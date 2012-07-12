﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Collections;
using Communication;
using Communication.Translator;
using MASGAU.Location;
using MASGAU.Location.Holders;
namespace MASGAU.Backup {
    public class ABackupProgramHandler<L> : AProgramHandler<L> where L : ALocationsHandler {
        public string archive_name_override = null;

        private List<GameVersion> back_these_up = null;
        private List<DetectedFile> only_these_files = new List<DetectedFile>();

        public ABackupProgramHandler(List<GameVersion> backup_list, Interface new_iface)
            : this(new_iface) {
            back_these_up = backup_list;
        }
        public ABackupProgramHandler(GameVersion this_game, List<DetectedFile> only_these, string archive_name, Interface new_iface)
            : this(new_iface) {
            back_these_up = new List<GameVersion>();
            back_these_up.Add(this_game);

            if (only_these != null) {
                only_these_files = only_these;
            }
            archive_name_override = archive_name;
        }

        public ABackupProgramHandler(Interface new_iface)
            : base(new_iface) {
            this.DoWork += new DoWorkEventHandler(BackupProgramHandler_DoWork);
            this.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackupProgramHandler_RunWorkerCompleted);
        }

        void BackupProgramHandler_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            // Make this check for things errors so it can clean up after itself
            if (output_path == null)
                return;
            foreach (FileInfo delete_me in new DirectoryInfo(output_path).GetFiles("~*.tmp")) {
                try {
                    delete_me.Delete();
                } catch (Exception ex) {

                    TranslatingMessageHandler.SendError("DeleteError", ex, delete_me.Name);
                }
            }
        }

        string output_path;
        void BackupProgramHandler_DoWork(object sender, DoWorkEventArgs e) {
            if (Core.settings.IsBackupPathSet || archive_name_override != null) {
                if (archive_name_override != null)
                    output_path = Path.GetDirectoryName(archive_name_override);
                else
                    output_path = Core.settings.backup_path;


                IList<GameVersion> games;

                if (back_these_up != null && back_these_up.Count > 0) {
                    games = back_these_up;
                } else {
                    if (Games.detected_games_count == 0)
                        Games.detectGames();
                    games = Games.DetectedGames.Items;
                }

                if (games.Count > 0) {
                    ProgressHandler.value = 1;
                    ProgressHandler.max = games.Count;
                    TranslatingProgressHandler.setTranslatedMessage("GamesToBeBackedUpCount", games.Count.ToString());


                    foreach (GameVersion game in games) {
                        if (CancellationPending)
                            return;

                        //if(archive_name_override!=null)
                        //all_users_archive = new ArchiveHandler(new FileInfo(archive_name_override),game.id);

                        if (games.Count == 1) {
                            TranslatingProgressHandler.setTranslatedMessage("BackingUpSingleGame", game.Title);
                        } else {
                            TranslatingProgressHandler.setTranslatedMessage("BackingUpMultipleGames", game.Title, ProgressHandler.value.ToString(), games.Count.ToString());
                        }

                        List<DetectedFile> files;
                        if (only_these_files != null && only_these_files.Count > 0) {
                            files = only_these_files;
                        } else {
                            files = game.Saves.Flatten();
                            ;
                        }


                        Archive override_archive = null;

                        try {
                            DictionaryList<Archive, DetectedFile> backup_files = new DictionaryList<Archive, DetectedFile>();
                            foreach (DetectedFile file in files) {
                                ArchiveID archive_id;
                                Archive archive;
                                if (CancellationPending)
                                    return;

                                QuickHash hash = file.RootHash;

                                archive_id = new ArchiveID(game.id, file.owner, file.type, hash);

                                if (archive_name_override != null) {
                                    if (override_archive == null)
                                        override_archive = new Archive(new FileInfo(archive_name_override), new ArchiveID(game.id, file.owner, null, hash));
                                    archive = override_archive;
                                } else {
                                    if (Archives.Get(archive_id) == null) {
                                        if (file.owner != null) {
                                            Archives.Add(new Archive(output_path, new ArchiveID(game.id, file.owner, file.type, hash)));
                                        } else {
                                            Archives.Add(new Archive(output_path, new ArchiveID(game.id, null, file.type, hash)));
                                        }
                                    }
                                    archive = Archives.Get(archive_id);
                                }

                                backup_files.Add(archive, file);
                            }
                            if (CancellationPending)
                                return;

                            foreach (KeyValuePair<Archive, List<DetectedFile>> backup_file in backup_files) {
                                if (override_archive == null)
                                    backup_file.Key.backup(backup_file.Value, false);
                                else
                                    backup_file.Key.backup(backup_file.Value, true);
                            }

                        } catch (Exception ex) {
                            TranslatingMessageHandler.SendException(ex);
                        } finally {
                            ProgressHandler.value++;
                        }
                    }
                } else {
                    TranslatingMessageHandler.SendError("NothingToBackup");
                }
            } else {
                TranslatingMessageHandler.SendError("BackupPathNotSet");
            }
        }

    }
}
