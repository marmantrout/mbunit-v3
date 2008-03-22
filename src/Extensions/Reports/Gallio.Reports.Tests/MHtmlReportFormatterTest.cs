// Copyright 2005-2008 Gallio Project - http://www.gallio.org/
// Portions Copyright 2000-2004 Jonathan De Halleux, Jamie Cansdale
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Gallio.Framework;
using Gallio.Hosting.ProgressMonitoring;
using Gallio.Runner.Reports;
using Gallio.Tests;
using Gallio.Utilities;
using MbUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;

namespace Gallio.Reports.Tests
{
    [TestFixture]
    [TestsOn(typeof(MHtmlReportFormatter))]
    public class MHtmlReportFormatterTest : BaseUnitTest
    {
        private delegate void FormatDelegate(IReportWriter reportWriter, NameValueCollection formatterOptions, IProgressMonitor progressMonitor);

        [Test, ExpectedArgumentNullException]
        public void NameCannotBeNull()
        {
            new MHtmlReportFormatter(null, "description", Mocks.Stub<IReportFormatter>());
        }

        [Test, ExpectedArgumentNullException]
        public void DescriptionCannotBeNull()
        {
            new MHtmlReportFormatter("name", null, Mocks.Stub<IReportFormatter>());
        }

        [Test, ExpectedArgumentNullException]
        public void HtmlReportFormatterCannotBeNull()
        {
            new MHtmlReportFormatter("name", "description", null);
        }

        [Test]
        public void FormatWritesTheArchivedReport()
        {
            IReportWriter reportWriter = Mocks.CreateMock<IReportWriter>();
            IReportContainer reportContainer = Mocks.CreateMock<IReportContainer>();
            IReportFormatter htmlReportFormatter = Mocks.CreateMock<IReportFormatter>();
            IProgressMonitor progressMonitor = NullProgressMonitor.CreateInstance();
            NameValueCollection options = new NameValueCollection();

            string reportPath = Path.GetTempFileName();
            using (Stream tempFileStream = File.OpenWrite(reportPath))
            {
                using (Mocks.Record())
                {
                    SetupResult.For(reportWriter.ReportContainer).Return(reportContainer);
                    SetupResult.For(reportWriter.Report).Return(new Report());

                    Expect.Call(reportContainer.EncodeFileName(null))
                        .Repeat.Any()
                        .IgnoreArguments()
                        .Do((Func<string, string>)delegate(string value) { return value; });

                    SetupResult.For(reportContainer.ReportName).Return("Foo");
                    Expect.Call(reportContainer.OpenWrite("Foo.mht", MimeTypes.MHtml, Encoding.UTF8))
                        .Return(tempFileStream);
                    reportWriter.AddReportDocumentPath("Foo.mht");

                    Expect.Call(delegate { htmlReportFormatter.Format(null, null, null); })
                        .Constraints(Is.NotNull(), Is.Same(options), Is.NotNull())
                        .Do((FormatDelegate)delegate(IReportWriter innerReportWriter, NameValueCollection innerFormatterOptions, IProgressMonitor innerProgressMonitor)
                        {
                            using (StreamWriter contentWriter = new StreamWriter(innerReportWriter.ReportContainer.OpenWrite("Foo.html", MimeTypes.Html, Encoding.UTF8)))
                                contentWriter.Write("<html><body>Some HTML</body></html>");

                            using (StreamWriter contentWriter = new StreamWriter(innerReportWriter.ReportContainer.OpenWrite(
                                innerReportWriter.ReportContainer.EncodeFileName("Foo\\Attachment 1%.txt"), MimeTypes.PlainText, Encoding.UTF8)))
                                contentWriter.Write("An attachment.");

                            using (StreamWriter contentWriter = new StreamWriter(innerReportWriter.ReportContainer.OpenWrite("Foo.css", null, null)))
                                contentWriter.Write("#Some CSS.");
                        });
                }

                using (Mocks.Playback())
                {
                    MHtmlReportFormatter formatter = new MHtmlReportFormatter("SomeName", "description", htmlReportFormatter);

                    formatter.Format(reportWriter, options, progressMonitor);

                    string reportContents = File.ReadAllText(reportPath);
                    Log.AttachPlainText("MHTML Report", reportContents);

                    Assert.Contains(reportContents, "MIME-Version: 1.0");
                    Assert.Contains(reportContents, "Content-Type: multipart/related; type=\"text/html\"; boundary=");
                    Assert.Contains(reportContents, "This is a multi-part message in MIME format.");

                    Assert.Contains(reportContents, "text/html");
                    Assert.Contains(reportContents, "Content-Location: file:///Foo.html");
                    Assert.Contains(reportContents, Convert.ToBase64String(Encoding.UTF8.GetBytes("<html><body>Some HTML</body></html>"), Base64FormattingOptions.InsertLineBreaks));

                    Assert.Contains(reportContents, "text/plain");
                    Assert.Contains(reportContents, "Content-Location: file:///Foo/Attachment_1%25.txt");
                    Assert.Contains(reportContents, Convert.ToBase64String(Encoding.UTF8.GetBytes("An attachment."), Base64FormattingOptions.InsertLineBreaks));

                    Assert.Contains(reportContents, "text/css");
                    Assert.Contains(reportContents, "Content-Location: file:///Foo.css");
                    Assert.Contains(reportContents, Convert.ToBase64String(Encoding.UTF8.GetBytes("#Some CSS."), Base64FormattingOptions.InsertLineBreaks));

                    File.Delete(reportPath);
                }
            }
        }
    }
}