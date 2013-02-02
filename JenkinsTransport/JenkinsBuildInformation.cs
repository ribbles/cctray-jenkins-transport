﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace JenkinsTransport
{
    public class JenkinsBuildInformation
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1).ToLocalTime();

        public DateTime Timestamp { get; private set; }
        public string Number { get; private set; }
        public int Duration { get; private set; }
        public int EstimatedDuration { get; private set; }
        public string FullDisplayName { get; private set; }
        public string Id { get; private set; }

        public JenkinsBuildInformation(XContainer document)
        {
            var firstElement = document.Element("freeStyleBuild");
            Timestamp = Epoch.AddMilliseconds((long) firstElement.Element("timestamp"));
            Number = (string) firstElement.Element("number");
            Duration = (int) firstElement.Element("duration");
            EstimatedDuration = (int) firstElement.Element("estimatedDuration");
            FullDisplayName = (string) firstElement.Element("fullDisplayName");
            Id = (string) firstElement.Element("id");
        }
    }
}
