﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MASGAU.Location.Holders;
namespace MASGAU.Location {
    public class DetectedLocations : Dictionary<string,DetectedLocationPathHolder>, IEnumerable<DetectedLocationPathHolder> {

        public DetectedLocationPathHolder getMostAccurateLocation() {
            DetectedLocationPathHolder candidate = null;
            foreach (DetectedLocationPathHolder path in this.Values) {
                if (candidate == null || candidate.rel_root < path.rel_root)
                    candidate = path;
            }
            return candidate;
        }

        public void Add(DetectedLocationPathHolder path) {
            // This compares the environment variables to ensure that the most accurate location gets used when the same path is entered twice
            string key = path.full_dir_path;
            if (this.ContainsKey(key)) {
                DetectedLocationPathHolder other = this[key];
                if (path.rel_root > other.rel_root)
                    this[key] = path;
            } else {
                base.Add(key, path);
            }
        }

        public new void Add(string name, DetectedLocationPathHolder path) {
            throw new NotImplementedException("Use single-value add instead");
        }


        public void AddRange(IEnumerable<DetectedLocationPathHolder> items) {
            foreach (DetectedLocationPathHolder item in items) {
                this.Add(item);
            }

        }

        public new IEnumerator<DetectedLocationPathHolder> GetEnumerator() {
            return this.Values.GetEnumerator();
        }
    }
}
