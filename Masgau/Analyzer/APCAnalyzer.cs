﻿using System;
using System.IO;
using System.ComponentModel;
using MVC.Communication;
using MVC.Translator;
using MASGAU.Location;
using MASGAU.Location.Holders;
using GameSaveInfo;
namespace MASGAU.Analyzer {
    public class APCAnalyzer : AAnalyzer {
        protected DetectedLocationPathHolder path;
        protected APCAnalyzer(CustomGameEntry game, RunWorkerCompletedEventHandler when_done)
            : base(game,when_done) {
        }

        protected override void analyzerWork() {
            outputLine("Game Name: " + game.Title);

            this.path = game.DetectedLocations.getMostAccurateLocation();



            if (path.rel_root == EnvironmentVariable.VirtualStore) {
                string drive = Path.GetPathRoot(path.full_dir_path);
                string new_path = Path.Combine(drive, path.Path);
                this.path = Core.locations.interpretPath(new_path).getMostAccurateLocation();
            }

            string[] folders = this.path.Path.Split(System.IO.Path.DirectorySeparatorChar);
            path.Path = folders[0];


            outputLine("Operating System: ");
            outputLine(Environment.OSVersion.VersionString);


            ProgressHandler.max += 4;
            outputLine();
            outputLine("Path: ");
            outputPath(path.full_dir_path);
            outputLine();
            try {
                TranslatingProgressHandler.setTranslatedMessage("AnalyzingScummVM");
                ProgressHandler.value++;
                outputLine(Environment.NewLine + "ScummVM Path Entries: ");
                scanForScumm(path.full_dir_path);
            } catch (Exception ex) {
                outputLine("Error while attempting to cehck for ScummVM path entries:");
                recordException(ex);
            }
            try {
                TranslatingProgressHandler.setTranslatedMessage("DumpingFolder");
                ProgressHandler.value++;
                outputLine(Environment.NewLine + "Folder Dump: ");
                travelFolder(path.full_dir_path);
            } catch (Exception ex) {
                outputLine("Error while attempting to search the save folder:");
                recordException(ex);
            }

        }

        protected void scanForScumm(string save_path) {
            throw new NotImplementedException("Haven't configured for ScummVM yet");
        }

    }
}
