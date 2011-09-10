using CrowSoftware.Lib.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CrowSoftware.Common.Container;
using Castle.Windsor;
using Rhino.Mocks;
using CrowSoftware.Common;
using NLog;
using Castle.Core.Logging;
using CrowSoftware.Common.Log;
using System.Globalization;
using System.Text;
using System.Threading;

namespace CrowSoftware.Lib.Test
{
    
    
    /// <summary>
    ///This is a test class for AsyncTcpClientTest and is intended
    ///to contain all AsyncTcpClientTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AsyncTcpClientTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //

        private IContainerManager containerManager;
        private IWindsorContainer windsorContainer;
        private MockRepository mocks;
        private ILogger Logger;
        private ILogManager Log;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            mocks = new MockRepository();
            containerManager = ContainerUtil.Initialize(true);
            windsorContainer = ((IWindsorContainerManager)containerManager).WindsorContainer;
            LibInstaller.RegisterLogging(windsorContainer);
            containerManager.Register<IAsyncTcpClient, AsyncTcpClient>();
            Log = containerManager.GetInstance<ILogManager>();
            Logger = Log.GetLogger(typeof(AsyncTcpClientTest));

            //containerManager.Register<IMarqueeManager, MarqueeManager>();

            //map = mocks.StrictMock<IMarqueeCharacterMap>();
            //containerManager.Register<IMarqueeCharacterMap>(map);

            //device = mocks.StrictMock<IMarqueeDevice>();
            //containerManager.Register<IMarqueeDevice>(device);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            mocks = null;
            ContainerUtil.Dispose();
        }

        #endregion


        /// <summary>
        ///A test for AsyncTcpClient Constructor
        ///</summary>
        [TestMethod()]
        public void AsyncTcpClientConstructorTest()
        {
            IAsyncTcpClient target = containerManager.GetInstance<IAsyncTcpClient>();
            target.DataReceived += DataReceivedHandler;
            target.StateTransitioned += StateTransitionedHandler;

            target.Initialize("TEST", "localhost", 7);
            target.Connect();
            for (int x = 1; x < 30; x++)
            {
                target.Send(Encoding.ASCII.GetBytes("StringData" + x));
                Thread.Sleep(1000);
            }
            target.Disconnect();
        }

        void StateTransitionedHandler(object sender, AsyncTcpClientEventArgs e)
        {
            Logger.InfoFormat(CultureInfo.InvariantCulture, "Old State = {0}, New State = {1}", e.OldState, e.NewState);
        }

        void DataReceivedHandler(object sender, AsyncTcpClientDataReceivedArgs e)
        {
            Logger.InfoFormat(CultureInfo.InvariantCulture, "Received: {0}", ASCIIEncoding.ASCII.GetString(e.Data));
        }
    }
}
