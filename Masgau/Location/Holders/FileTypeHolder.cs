﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
namespace MASGAU.Location.Holders {
    public class FileTypeHolder:  List<SaveHolder> {
        public string Type { get; protected set; }

        public DetectedFiles Files {
            get {
                throw new NotSupportedException();
            }
        }
        public FileTypeHolder(string name) {
            this.Type = name;
        }
        public FileTypeHolder(XmlElement element) {
            foreach (XmlAttribute attr in element.Attributes) {
                switch (attr.Name) {
                    case "type":
                        this.Type = attr.Value;
                        break;
                    default:
                        throw new NotSupportedException(attr.Name);
                }
            }
            foreach (XmlElement child in element.ChildNodes) {
                switch (child.Name) {
                    case "save":
                        SaveHolder save = new SaveHolder(child, Type);
                        this.Add(save);
                        break;
                    default:
                        throw new NotSupportedException(child.Name);
                }
            }
        }

        public virtual List<DetectedFile> FindMatching(DetectedLocationPathHolder location) {
            List<DetectedFile> files = new List<DetectedFile>();
            foreach (SaveHolder save in this) {
                files.AddRange(save.FindMatching(location));
            }
            return files;

        }

    }
}
