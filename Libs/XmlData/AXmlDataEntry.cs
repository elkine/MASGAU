﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
namespace XmlData {
    public abstract class AXmlDataEntry {
        protected XmlDocument doc = null;

        public abstract string ElementName { get; }

        public XmlElement XML {
            get {
                if (this.xml == null) {
                    this.xml = createXml();
                }
                return this.xml;
            }
            protected set {
                XML = value;
            }
        }

        protected XmlElement xml = null;
        protected AXmlDataEntry() { }

        public AXmlDataEntry(XmlDocument document) {
            this.doc = document;
        }

        public AXmlDataEntry(XmlElement element): this(element.OwnerDocument) {
            xml = element;
            LoadData(element);
        }

        public XmlElement addAtribute(XmlElement ele, string name, string contents) {
            if (contents == null)
                return ele;

            XmlAttribute att= this.doc.CreateAttribute(name);
            att.Value = contents;
            ele.Attributes.Append(att);
            return ele;
        }

        public XmlElement createElement(string name, string contents) {
            XmlElement ele = createElement(name);
            ele.InnerText = contents;
            return ele;
        }
        public XmlElement createElement(string name) {
            return this.doc.CreateElement(name);
        }

        private XmlElement createXml() {
            XmlElement ele = this.createElement(this.ElementName);
            WriteData(ele);
            return ele;
        }
        protected abstract void LoadData(XmlElement element);
        protected abstract XmlElement WriteData(XmlElement element);
    }
}
