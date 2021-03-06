﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Security.AccessControl;
using MASGAU.Location;
using MASGAU.Game;
using MASGAU.Location.Holders;
using MASGAU.Communication.Progress;
using MASGAU.Communication.Message;
using MASGAU.Communication.Request;

namespace MASGAU.Archive
{
    public class ArchiveHandler: AModelItem<ArchiveID>
    {
        #region Archive identification stuff

        public string title {
            get {
                GameHandler game = Core.games.all_games.get(this.id.game);
                StringBuilder return_me = new StringBuilder();
                if(game==null) {
                    return_me.Append(id.game.ToString());
                } else {
                    return_me.Append(game.title);
                    if(game.platform!=null)
                        return_me.Append(" - " + game.platform.ToString());
                    if(game.id.country!=null)
                        return_me.Append(" - " + game.id.country);
                }
                return return_me.ToString();
            }
        }

        public string owner {
            get {
                return id.owner;
            }
        }
        public string type {
            get {
                return id.type;
            }
        }

        private readonly String _file_name;
        public string file_name { 
            get {
                return _file_name;
            }
        }
        public string temp_file_name {
            get {
                String name = Path.Combine(Path.GetDirectoryName(_file_name),"~" + Path.GetFileNameWithoutExtension(_file_name) + ".tmp");
                return name;
            }
        }

        public DateTime file_date {get; set;}
        public string game {
            get {
                return id.ToString();
            }
        }
        public string game_name {
            get { return id.game.name;}
        }
        public string game_platform {
            get { return id.game.platform.ToString();}
        }
        public string game_country {
            get { return id.game.country;}
        }

        #endregion

        public bool exists {
            get {
                FileInfo file = new FileInfo(file_name);
                return file.Exists;
            }
        }


        #region static stuff
        private static bool ready;
        private static Process zipper;
        public static string temp_folder;
        private static XmlReaderSettings xml_settings = new XmlReaderSettings();
        static ArchiveHandler() {
            zipper = new Process();
            temp_folder = Path.Combine(Environment.GetEnvironmentVariable("TEMP"),"MASGAU");

            string path = Path.Combine(Core.app_path,"7z.exe");
            if(File.Exists(path)) {
                zipper.StartInfo.FileName = path;
                ready = true;
            } else {
                ready = false;
                throw new MException("Archiver Error","7z.exe Not Found In Program Folder.\nYou Will Probably Need To Reinstall.",false);
            }
            zipper.StartInfo.UseShellExecute = false;
            zipper.StartInfo.RedirectStandardOutput = true;
            zipper.StartInfo.RedirectStandardError = true;
            zipper.StartInfo.CreateNoWindow = true;
            zipper.StartInfo.WorkingDirectory = temp_folder;

            xml_settings.ConformanceLevel = ConformanceLevel.Document;
            xml_settings.IgnoreComments = true;
            xml_settings.IgnoreWhitespace = true;
        }
        private static void prepTemp() {
            try {
                if (Directory.Exists(temp_folder)) {
                    Directory.Delete(temp_folder,true);
                    Thread.Sleep(10);
                }
                Directory.CreateDirectory(temp_folder);
            } catch(Exception e) {
                throw new MException("Look Out!","An error occured while trying to prep " + temp_folder, e, false);
            }
        }
        #endregion

        #region Constructor
        public ArchiveHandler(string folder, ArchiveID new_id):
            this(new FileInfo(Path.Combine(folder,new_id.ToString()) + Core.extension),new_id) {
        }

        public ArchiveHandler(FileInfo archive, ArchiveID new_id): 
            this(archive) {
            
            this.id = new_id;
        }

        public ArchiveHandler(FileInfo archive): base() {
            if(!ready)
                throw new MException("Not Found","This 7-zip executable could not be found. You probably need to re-install.", false);

            _file_name = archive.FullName;
            String owner = null;
            String name = null;
            // this is the default, so that older archives will still correctly identify as windows
            GamePlatform platform = GamePlatform.Windows;
            String country = null;
            String type = null;
            if(exists)
                file_date = archive.LastWriteTime;
            else // A red-letter date in the history of science
                file_date = new DateTime(1955, 11, 5);

            // If the archive is new, then the following won't apply
            if(!exists)
                return;

            // Attempts to extract only the masgau.xml identifying file from the game
            List<string> xml_file = new List<string>();
            xml_file.Add("masgau.xml");
            extract(xml_file,false);

            if(File.Exists(Path.Combine(temp_folder, "masgau.xml"))) {
                XmlReader load_me = XmlReader.Create(Path.Combine(temp_folder, "masgau.xml"),xml_settings);
                while (load_me.Read()) {
    			    if(load_me.NodeType==XmlNodeType.Element) {
				        switch (load_me.Name) {
                            case "game":
                                while(load_me.MoveToNextAttribute()) {
                                    switch(load_me.Name) {
                                        case "name":
                                            name = load_me.Value;
                                            break;
                                        case "platform":
                                            platform = GameHandler.parseGamePlatform(load_me.Value);
                                            break;
                                        case "country":
                                            country= load_me.Value;
                                            break;
                                    }
                                }
                                break;
                            case "owner":
                                while(load_me.MoveToNextAttribute()) {
                                    switch(load_me.Name) {
                                        case "name":
                                            owner = load_me.Value;
                                            break;
                                    }
                                }
                                break;
                            case "archive":
                                while(load_me.MoveToNextAttribute()) {
                                    switch(load_me.Name) {
                                        case "type":
                                            type = load_me.Value;
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                }
                this.id = new ArchiveID(new GameID(name,platform,country),owner,type);
                load_me.Close();
                File.Delete(Path.Combine(temp_folder, "masgau.xml"));
            } else {
                throw new MException("Lack Of Data Is Bad","The file " + file_name + " doesn't contain any MASGAU data.\nThis probably means it's a corrupted archive, or not an archive at all.\nPlease move it out of the backup folder or delete it.",false);
                // This is a fallback for really old ASGAU archives - One day I will be able to just delete this.
                // I'm going to delete this actually
                // Goodbye, legacy!!!!
                //hold_me = file_name.Replace(".gb7", "").Split('@');
                //hold_me = hold_me[0].Split('«');
                //id.name = hold_me[0].Trim(); ;
                //id.platform = GamePlatform.Windows;
                //if(hold_me.Length>1) {
                //    owner = hold_me[1].Trim();
                //} else {
                //    owner = null;
                //}
            }
        }
        #endregion

        #region Backup Stuff
        // This helps determin wether this is the first time the archive has been backed up to
        // it's so we can bypass the date checks as they will not be accurate anymore
        public void backup(ICollection<DetectedFile> files, bool disable_versioning) {
            prepTemp();


            if(exists) {
                if(!canWrite(new FileInfo(file_name)))
                    throw new MException("Gadzooks!","The file " + file_name + " is not writable", false);
            } else {
                if(!canWrite(new DirectoryInfo(Path.GetDirectoryName(file_name))))
                    throw new MException("Gadzooks!","The folder " + Path.GetDirectoryName(file_name) + " is not writable", false);

                // If this is the first time writing to the archive, we create the identifying XML
                XmlDocument write_me = new XmlDocument();
                XmlElement node;
                XmlAttribute attribute;
                if(File.Exists(Path.Combine(temp_folder,"masgau.xml")))
                    File.Delete(Path.Combine(temp_folder,"masgau.xml"));

                XmlTextWriter write_here = new XmlTextWriter(Path.Combine(temp_folder,"masgau.xml"), System.Text.Encoding.UTF8);
                write_here.Formatting = Formatting.Indented;
                write_here.WriteProcessingInstruction("xml","version='1.0' encoding='UTF-8'");
                write_here.WriteStartElement("masgau_archive");
                write_here.Close();
                write_me.Load(Path.Combine(temp_folder,"masgau.xml"));
                //XmlNode root = write_me.DocumentElement;

                node = write_me.CreateElement("game");
                attribute = write_me.CreateAttribute("name");
                attribute.Value = id.game.name;
                node.SetAttributeNode(attribute);
                attribute = write_me.CreateAttribute("platform");
                attribute.Value = id.game.platform.ToString();
                node.SetAttributeNode(attribute);
                if(id.game.country!=null ) {
                    attribute = write_me.CreateAttribute("country");
                    attribute.Value = id.game.country;
                    node.SetAttributeNode(attribute);
                }

                write_me.DocumentElement.InsertAfter(node, write_me.DocumentElement.LastChild);

                if(id.owner!=null) {
                    node = write_me.CreateElement("owner");
                    attribute = write_me.CreateAttribute("name");
                    attribute.Value = id.owner;
                    node.SetAttributeNode(attribute);
                    write_me.DocumentElement.InsertAfter(node, write_me.DocumentElement.LastChild);
                }

                if(id.type!= null) {
                    node = write_me.CreateElement("archive");
                    attribute = write_me.CreateAttribute("type");
                    attribute.Value = id.type.ToString();
                    node.SetAttributeNode(attribute);
                    write_me.DocumentElement.InsertAfter(node, write_me.DocumentElement.LastChild);
                }

                FileStream this_file = new FileStream(Path.Combine(temp_folder,"masgau.xml"), FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);

                write_me.Save(this_file);
                write_here.Close();
                this_file.Close();
            }
            bool files_added = false;
            foreach(DetectedFile file in files) {
                // Copies the particular file to a relative path inside the temp folder
                string from_here, in_here, to_here;
                if (file.path == "") {
                    from_here = file.abs_root;
                    in_here = file.name;
                } else {
                    from_here = Path.Combine(file.abs_root, file.path);
                    in_here = Path.Combine(file.path, file.name);
                }

                from_here = Path.Combine(from_here,file.name);
                to_here = Path.Combine(temp_folder, in_here);

                DateTime file_write_time = File.GetLastWriteTime(from_here);
                int time_comparison = DateTime.Compare(file_date,file_write_time);
                if(Core.settings.ignore_date_check||time_comparison<=0) {
                    try {
                        if (!Directory.Exists(Path.GetDirectoryName(to_here)))
                            Directory.CreateDirectory(Path.GetDirectoryName(to_here));
                    } catch(Exception e) {
                        MessageHandler.SendError("I Am The High Commander","An error occured when trying to create " + Path.GetDirectoryName(to_here),e);
                        continue;
                    }

                    if(File.Exists(from_here)) {
                        try {
                            File.Copy(from_here, to_here, true);
                        } catch(Exception e) {
                            MessageHandler.SendError("Eccentric!","An error occured while trying to copy " + from_here,e);
                            continue;
                        } 
                    } else {
                        MessageHandler.SendError("This May Or May Not Be Important","A file that was supposed to be copied could not be found",from_here);
                        continue;
                    }

                    if(File.Exists(to_here)) {
                        try {
                            if ((File.GetAttributes(to_here) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                                File.SetAttributes(to_here, FileAttributes.Normal);
                        } catch(Exception e) {
                            MessageHandler.SendError("I'm a program, not a red shirt","An error occcured while trying to change the permissions on " + to_here,e);
                            continue;
                        }
                    } else {
                        MessageHandler.SendError("This May Or May Not Be Important","Okay so we copied a file to somewhere else and nothing went wrong,\nbut the file isn't where we copied it to :/",to_here);
                        continue;
                    }
                    files_added = true;
                }
                //file_date = new FileInfo(file_name).LastWriteTime;
            }
            if(files_added)
                add(temp_folder,disable_versioning);
            Directory.Delete(temp_folder, true);
        }

        #endregion

        #region Restore stuff
        public bool cancel_restore = false;
        public void restore(DirectoryInfo destination, List<string> only_these) {
            cancel_restore = false;
            ProgressHandler.progress_message = "Checking Destination...";
            // The files get extracted to the temp folder, so this sets up our ability to read them
            DirectoryInfo from_here = new DirectoryInfo(temp_folder);

            // If it's a destination that doesn't yet exist
			if(!destination.Exists) {
				try {
					destination.Create();
                    // This sets the permissions on the folder to be for everyone
					DirectorySecurity everyone = destination.GetAccessControl();
					string everyones_name = @"Everyone";
					everyone.AddAccessRule(new FileSystemAccessRule(everyones_name,FileSystemRights.FullControl,InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit,PropagationFlags.None,AccessControlType.Allow));
					destination.SetAccessControl(everyone);
				} catch {
                    // This is if the folder creation fails
                    // It means we have to run as admin, but we check here if we already are
				    if(!SecurityHandler.amAdmin()) {
                        // If we aren't we elevate ourselves
                        restoreElevation(destination.FullName);
                        cancel_restore = true;
				    } else {
                        throw new MException("And that's how it is","Unable to create output folder:" + Environment.NewLine + destination.FullName,false);
				    }
				}
			}
            if(cancel_restore)
                return;

            ProgressHandler.progress_message = "Extracting Archive...";
            ProgressHandler.progress_state = ProgressState.Indeterminate;
            if(only_these==null) {
                ProgressHandler.progress_max = file_count;
                extract(true);
            } else {
                ProgressHandler.progress_max = only_these.Count;
                extract(only_these,true);
            }
            ProgressHandler.progress = 0;
            ProgressHandler.progress_state = ProgressState.Normal;

            // Clean up the masgau archive ID
            if(File.Exists(Path.Combine(temp_folder,"masgau.xml")))
                File.Delete(Path.Combine(temp_folder,"masgau.xml"));

            if(cancel_restore)
                return;

            ProgressHandler.progress_message = "Copying Files To Destination...";
            if(!canWrite(destination)) {
	            Directory.Delete(temp_folder, true);
                restoreElevation(destination.FullName);
                cancel_restore = true;
	        }
            if(cancel_restore)
                return;

            copyFolders(from_here, destination,true);

            if(cancel_restore)
                return;

            ProgressHandler.progress_state = ProgressState.None;
        }
        private bool restoreElevation(string destination) {
            try {
                if(SecurityHandler.amAdmin()) {
                    Communication.Message.MessageHandler.SendError("Nutbunnies","Unable to create output folder:" + Environment.NewLine + destination + Environment.NewLine + "MASGAU is already trying in admin mode, so that means that your user is specifically denied access to this folder. Sorry.");
                    return false;
                }

                Communication.Interface.InterfaceHandler.disableInterface();
                if(!RequestHandler.Request(RequestType.Question,"Cheese and Crackers","Unable to create output folder:" + Environment.NewLine + destination + Environment.NewLine + "Would you like to try again as admin?").cancelled){
				    try {
                        SecurityHandler.elevation(Core.programs.restore,"\"" + file_name + "\"");
                        return true;
				    }
				    catch (Exception e){
                        throw new MException("People people people get up get up",Core.programs.restore + "can't be found.\nYou probably need to reinstall.",e, false);
				    }
			    } else {
                    return false;
                }
            } finally {
                Communication.Interface.InterfaceHandler.enableInterface();
                Communication.Interface.InterfaceHandler.closeInterface();
            }
        }


        #endregion

        #region 7-zip stuff
        // Compression-related options
        protected static string compress_mode = "a";
        protected static string file_type = "7z";
        protected static bool solid_mode = false;
        protected static bool multi_threading = true;
        protected static int compress_level = 9;

        private static string compress_switches {
            get {
                // Sets the mode and file type
                StringBuilder return_me = new StringBuilder(compress_mode + " -t" + file_type);
                // Solid mode is on by default in 7zip
                if(!solid_mode)
                    return_me.Append(" -ms=off");
                
                //Compression level
                return_me.Append(" -mx=" + compress_level.ToString());

                if(multi_threading)
                    return_me.Append(" -mmt=on");
                else
                    return_me.Append(" -mmt=off");
                return return_me.ToString();
            }
        }
        private void run7z(bool send_progress) {
            if(this.exists) {
                string status = ProgressHandler.progress_message;
                int wait_max = 30;
                for(int count = 0; !canRead(file_name); count++) {
                    ProgressHandler.progress_message = status.TrimEnd('.') + " - Archive In Use, Waiting (" + (count+1) + "/" + wait_max + ")";
                    Thread.Sleep(1000);
                    if(count==wait_max-1)
                        throw new MException("Tired of waiting!","The archive " + file_name + Environment.NewLine + "has been unaccessible for 30 seconds, it will be skipped.",false);
                }
            }
            try {
                zipper.Start();
                string output = zipper.StandardOutput.ReadLine();
                while(output != null){
                    if(send_progress) {
                        if(output.StartsWith("Extracting")||output.StartsWith("Compressing"))
                            ProgressHandler.progress++;
                    }
                    output = zipper.StandardOutput.ReadLine();
                }
            } catch(Exception e) {
                throw new MException("Running is for wimps","An error occured while trying to run 7-zip with the arguments:\n" + zipper.StartInfo.Arguments,e, true);
            } finally {
                try {
                    zipper.Close();
                } catch {}
            }
        }

        private void add(string file, bool disable_versioning) {
            if(File.Exists(temp_file_name))
                File.Delete(temp_file_name);

            if(this.exists) {
                File.Copy(file_name,temp_file_name);
                File.SetAttributes(temp_file_name, FileAttributes.Hidden);
            }

            zipper.StartInfo.Arguments = compress_switches + " \"" + temp_file_name + "\" \"" + file + "\\*\"";
            run7z(false);

            File.SetAttributes(temp_file_name, FileAttributes.Hidden);

            // This here's the versioning stuff. Since it's here, it's universal.
            // This handles versioning copies
            if(!disable_versioning&&Core.settings.versioning) {
                if(Core.settings.versioning_max!=0) {
                    DateTime right_now = DateTime.Now;
                    FileInfo original_file = new FileInfo(file_name);
                    if(original_file.Exists) {
                        if(right_now.Ticks-original_file.CreationTime.Ticks>Core.settings.versioning_ticks) {
                            string new_path = Path.Combine(original_file.DirectoryName,Path.GetFileNameWithoutExtension(original_file.FullName) + "@" + right_now.ToString().Replace('/','-').Replace(':','-') + Path.GetExtension(original_file.FullName));
                            try {
                                File.Move(file_name, new_path);
                                //File.SetCreationTime(temp_file_name,right_now);
                            } catch(Exception ex) {
                                throw new MException("Versioning is for wusses anyway","An error occured while trying to make a revision copy of " + original_file.FullName, ex,false);
                            }
                        } else {
                            // This is if it hasn't been long enough for a new file
                        }
                    } else {
                        // This shouldn't really be an error, as it just means that it's a new archive
                        //MessageHandler.SendError("Where'd That Come From?","The file " + file_name + " can't be found.");
                    }
                } else {
                    //this is if the Settings for versioning are fucked up
                }

                FileInfo[] count_us = new DirectoryInfo(Path.GetDirectoryName(file_name)).GetFiles(
                    Path.GetFileNameWithoutExtension(file_name) + "@*");


                if(count_us.Length>Core.settings.versioning_max) {
                    Array.Sort(count_us,new MASGAU.Comparers.FileInfoComparer(true));
                    for(long i = Core.settings.versioning_max;i<count_us.Length;i++) {
                        try {
                            (count_us[i] as FileInfo).Delete();
                        } catch(Exception ex) {
                            MessageHandler.SendError("Slathered all over it","An error occured while trying to delete " + (count_us[i] as FileInfo).Name, ex);
                        }
                    }
                }
            } else {
                // This is if versioning is disabled, or something

            }

            if(File.Exists(file_name))
                File.Delete(file_name);

            File.Move(temp_file_name,file_name);
            File.SetAttributes(file_name, FileAttributes.Normal);
        }


        private string list_switches = "l";
        public int file_count {
            get {
                List<string> lines = getFileListing();

                string[] hold_this = lines[lines.Count-1].Substring(53).Split(' ');

                return Int32.Parse(hold_this[0]);
            }
        }
        public List<string> file_list {
            get {
                List<string> return_me = new List<string>();

                List<string> lines = getFileListing();
                bool file_list_started = false;
                foreach(string line in lines) {
                    if(line.StartsWith("-----")) {
                        file_list_started = true;
                        continue;
                    } else if(line.StartsWith("     ")) {
                        file_list_started = false;
                    }
                    if(file_list_started&&line.Substring(20,1)!="D") {
                        string add_me = line.Substring(53);
                        if(add_me!="masgau.xml")
                            return_me.Add(add_me);
                    }
                }

                return return_me;
            }
        }

        private List<string> getFileListing()  {
            List<String> return_me = new List<string>();
            zipper.StartInfo.Arguments = list_switches + " \"" + file_name + "\"";
            string old_dir = zipper.StartInfo.WorkingDirectory;
            zipper.StartInfo.WorkingDirectory = "";
            zipper.Start();

            string output = zipper.StandardOutput.ReadLine();
            while(output!=null) {
                return_me.Add(output);
                output = zipper.StandardOutput.ReadLine();
            }

            zipper.Close();

            zipper.StartInfo.WorkingDirectory = old_dir;

            return return_me;
        }

        private string extract_switches = "x";
        private void extract(bool send_progress) {
            prepTemp();
            zipper.StartInfo.Arguments = extract_switches + " \"" + file_name + "\"";
            run7z(send_progress);
        }
        private void extract(List<string> files, bool send_progress) {
            prepTemp();
            if(send_progress) {
                ProgressHandler.progress_max = files.Count;
                ProgressHandler.progress = 0;
            }
            foreach(string file in files) {
                zipper.StartInfo.Arguments = extract_switches + " \"" + this.file_name + "\" \"" + file + "\"";
                run7z(false);
                if(send_progress)
                    ProgressHandler.progress++;
            }
        }
        #endregion

        #region Helper methods
        // Returns a list of the files in the archive


        // Static method for copying folders
        public bool copyFolders(DirectoryInfo from_here, DirectoryInfo to_here, bool send_progress) {
            DirectoryInfo now_here;
            if(!to_here.Exists){
	            try {
		            Directory.CreateDirectory(to_here.FullName);
				} catch (Exception e) {
                    throw new MException("My genetics are repressed","Error while creating " + to_here.FullName,e,false);
				}
			}
            foreach(FileInfo copy_me in from_here.GetFiles()) {
                if(cancel_restore)
                    break;
                if(send_progress)
                    ProgressHandler.progress++;
				try {
					File.Copy(copy_me.FullName,Path.Combine(to_here.FullName,copy_me.Name),true);
                    if ((File.GetAttributes(Path.Combine(to_here.FullName,copy_me.Name)) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        File.SetAttributes(Path.Combine(to_here.FullName,copy_me.Name), FileAttributes.Normal);
				} catch (Exception e) {
                    throw new MException("We started this Op'ra","Error while copying " + copy_me.FullName + "\nto " + to_here.FullName, e, false);
				}
            }
            foreach(DirectoryInfo check_me in from_here.GetDirectories()) {
                if(cancel_restore)
                    break;
                now_here = new DirectoryInfo(Path.Combine(to_here.FullName,check_me.Name));
                if(!copyFolders(check_me, now_here,send_progress))
                    return false;
            }
            return true;
        }


        // Static methods for determining access
        public static bool canRead(string file) {
            return canRead( new FileInfo(file));
        }
        public static bool canRead(FileInfo file) {
            try {
                FileStream close_me = file.Open(FileMode.Open,FileAccess.Read);
                close_me.Close();
                return true;
            } catch (Exception e) {
                return false;
            }
        }
        public static bool canWrite(DirectoryInfo path) {
            if(!path.Exists) {
                try {
                    //createPath(path); WTF was I thinking? This shall remain here out of shame.
                    path.Create();
                } catch (Exception e) {
                    return false;
                }
            }

            path.Refresh();

            if (path.Exists) {
                string file_name = "~" + Path.GetRandomFileName() + ".tmp";
                FileInfo test_file = new FileInfo(Path.Combine(path.FullName,file_name));
                try {
                    FileStream delete_me = test_file.Create();
                    delete_me.Close();
                } catch (Exception e) {
                    return false;
                }
                for(int i = 0;i<=5;i++) {
                    try {
                        test_file.Delete();
                        return true;
                    } catch (Exception e) {
                        Thread.Sleep(5000);
                    }
                }
            }
            return false;
        }
        public static bool canWrite(FileInfo file) {
            try {
                if(file.Exists) {
                    FileStream close_me = file.Open(FileMode.OpenOrCreate,FileAccess.ReadWrite);
                    close_me.Close();
                } else {
                    FileStream close_me = file.Open(FileMode.OpenOrCreate,FileAccess.ReadWrite);
                    close_me.Close();
                    file.Delete();
                }
                return true;
            } catch (Exception e) {
                return false;
            }
        }
        #endregion

    }
}
