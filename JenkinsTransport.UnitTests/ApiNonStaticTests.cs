﻿using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Net;
using System.Collections.Generic;
using FluentAssertions;
using JenkinsTransport.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ThoughtWorks.CruiseControl.Remote;


namespace JenkinsTransport.UnitTests
{
    internal class ApiNonStaticTestDependencies
    {
        public Mock<IWebRequestFactory> MockWebRequestFactory;
        public IWebRequestFactory WebRequestFactory { get { return MockWebRequestFactory.Object; }}

        public ApiNonStaticTestDependencies()
        {
            MockWebRequestFactory = new Mock<IWebRequestFactory>();
        }

        public void UseThisFileAsResponse(string sampleData)
        {
            Stream responseStream = new FileStream(sampleData, FileMode.Open);

            var webResponse = new TestWebReponse(responseStream);
            Mock<IWebRequest> mockWebRequest = new Mock<IWebRequest>();
            mockWebRequest
                .Setup(x => x.GetResponse())
                .Returns(webResponse);

            MockWebRequestFactory
               .Setup(x => x.Create(It.IsAny<string>()))
               .Returns(mockWebRequest.Object);
        }
    }

    [TestClass]
    public class ApiNonStaticTests
    {
        protected string ProjectUrl = "https://builds.apache.org/job/Hadoop-1-win/api/xml";
        protected string ProjectName = "Hadoop-1-win";

        private ApiNonStatic CreateTestTarget()
        {
            var target = new ApiNonStatic("https://builds.apache.org/", String.Empty);
            return target;
        }

        private ApiNonStatic CreateTestTarget(ApiNonStaticTestDependencies dependencies)
        {
            var target = new ApiNonStatic("https://builds.apache.org/", String.Empty);

            target.WebRequestFactory = dependencies.WebRequestFactory;

            return target;
        }


        [TestInitialize]
        public void Setup()
        {      
            //ApiInstance = new ApiNonStatic("https://builds.apache.org/", String.Empty);
        }

        [TestMethod]
        [Ignore]
        public void RealGetAllJobs()
        {
            ApiNonStatic target = CreateTestTarget();

            target.WebRequestFactory = new WebRequestFactory();

            var jobs = target.GetAllJobs();
            Assert.IsNotNull(jobs);
            Assert.IsTrue(jobs.Any());
            CollectionAssert.AllItemsAreUnique(jobs);
        }

        /// <summary>
        /// Used to collect sample data for tests
        /// </summary>
        [TestMethod]
        [Ignore]
        public void RealGetProjectStatus()
        {
            var target = CreateTestTarget();
            target.WebRequestFactory = new WebRequestFactory();
            var jobs = target.GetAllJobs();
             
            var doc = target.GetProjectStatusDoc(ProjectUrl);
            doc.Save("ProjectStatusSampleData1.xml");

            Assert.IsNotNull(jobs);
            Assert.IsTrue(jobs.Any());
            CollectionAssert.AllItemsAreUnique(jobs);
        }

        [TestMethod]
        public void GetAllJobs_should_return_correct_number()
        {
            ApiNonStaticTestDependencies mocks = new ApiNonStaticTestDependencies();
            var target = CreateTestTarget(mocks);

            mocks.UseThisFileAsResponse(@".\TestData\TestJobsSampleData1.xml");       

            // Act
            var jobs = target.GetAllJobs();

            // Assert
            jobs.Count.Should().Be(1076);
            CollectionAssert.AllItemsAreUnique(jobs);
        }

        [TestMethod]
        public void GetProjectStatus_should_have_correct_project_name()
        {
            ApiNonStaticTestDependencies mocks = new ApiNonStaticTestDependencies();
            var target = CreateTestTarget(mocks);

            mocks.UseThisFileAsResponse(@".\TestData\ProjectStatusSampleData1.xml");      

            // Act
            var status = target.GetProjectStatus(ProjectUrl, null);

            // Assert
            status.Name.Should().Be(ProjectName);
        }

        [TestMethod]
        public void GetProjectStatus_should_have_correct_webUrl()
        {
            ApiNonStaticTestDependencies mocks = new ApiNonStaticTestDependencies();
            var target = CreateTestTarget(mocks);

            mocks.UseThisFileAsResponse(@".\TestData\ProjectStatusSampleData1.xml");            

            // Act
            ProjectStatus status = target.GetProjectStatus(ProjectUrl, null);

            // Assert
            status.WebURL.Should().Be("https://builds.apache.org/job/Hadoop-1-win/");
        }

    }

    class TestWebRequest : WebRequest
    {
        MemoryStream requestStream = new MemoryStream();
        MemoryStream responseStream;

        public override string Method { get; set; }
        public override string ContentType { get; set; }
        public override long ContentLength { get; set; }

        /// <summary>Initializes a new instance of <see cref="TestWebRequest"/>
        /// with the response to return.</summary>
        public TestWebRequest(string response)
        {
            responseStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(response));
        }

        /// <summary>Returns the request contents as a string.</summary>
        public string ContentAsString()
        {
            return System.Text.Encoding.UTF8.GetString(requestStream.ToArray());
        }

        /// <summary>See <see cref="WebRequest.GetRequestStream"/>.</summary>
        public override Stream GetRequestStream()
        {
            return requestStream;
        }

        /// <summary>See <see cref="WebRequest.GetResponse"/>.</summary>
        public override WebResponse GetResponse()
        {
            return new TestWebReponse(responseStream);
        }
    }


    internal class TestWebReponse : WebResponse
    {
        Stream responseStream;

        /// <summary>Initializes a new instance of <see cref="TestWebReponse"/>
        /// with the response stream to return.</summary>
        public TestWebReponse(Stream responseStream)
        {
            this.responseStream = responseStream;
        }

        /// <summary>See <see cref="WebResponse.GetResponseStream"/>.</summary>
        public override Stream GetResponseStream()
        {
            return responseStream;
        }
    }
}
