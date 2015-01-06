using Gallio.Framework;
using Gallio.Model;
using MbUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;

namespace SauceLabs.SeleniumExamples
{
    /// <summary>tests for the sauce labs guinea pig page</summary>
    [TestFixture]
    [Header("browser", "version", "platform")] // name of the parameters in the rows
    [Row("internet explorer", "11", "Windows 7")] // run all tests in the fixture against IE 11 for windows 7
    [Row("chrome", "35", "linux")] // run all tests in the fixture against chrome 35 for linux
    [Row("safari","6","OS X 10.8")] // run all tests in the fixture against safari 6 and mac OS X 10.8
    public class GuineaPigTests
    {
        #region Setup and Teardown

        /// <summary>starts a sauce labs sessions</summary>
        /// <param name="browser">name of the browser to request</param>
        /// <param name="version">version of the browser to request</param>
        /// <param name="platform">operating system to request</param>
        private IWebDriver _Setup(string browser, string version, string platform)
        {
            // construct the url to sauce labs
            Uri commandExecutorUri = new Uri("http://ondemand.saucelabs.com/wd/hub");

            // set up the desired capabilities
            DesiredCapabilities desiredCapabilites = new DesiredCapabilities(browser, version, Platform.CurrentPlatform); // set the desired browser
            desiredCapabilites.SetCapability("platform", platform); // operating system to use
            desiredCapabilites.SetCapability("username", Constants.SAUCE_LABS_ACCOUNT_NAME); // supply sauce labs username
            desiredCapabilites.SetCapability("accessKey", Constants.SAUCE_LABS_ACCOUNT_KEY);  // supply sauce labs account key
            desiredCapabilites.SetCapability("name", TestContext.CurrentContext.Test.Name); // give the test a name

            // start a new remote web driver session on sauce labs
            var _Driver = new RemoteWebDriver(commandExecutorUri, desiredCapabilites);
            _Driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));

            // navigate to the page under test
            _Driver.Navigate().GoToUrl("https://saucelabs.com/test/guinea-pig");

            return _Driver;
        }


        /// <summary>called at the end of each test to tear it down</summary>
        public void CleanUp(IWebDriver _Driver)
        {
            // get the status of the current test
            bool passed = TestContext.CurrentContext.Outcome.Status == TestStatus.Passed;
            try
            {
                // log the result to sauce labs
                ((IJavaScriptExecutor)_Driver).ExecuteScript("sauce:job-result=" + (passed ? "passed" : "failed"));
            }
            finally
            {
                // terminate the remote webdriver session
                _Driver.Quit();
            }
        }

        #endregion

        #region Tests

        /// <summary>tests the title of the page</summary>
        [Test, Parallelizable] // denotes that this method is a test and can be run in parallel
        public void PageTitle(string browser, string version, string platform)
        {
            // start the remote webdriver session with sauce labs
            var _Driver = _Setup(browser, version, platform);

            // verify the page title is correct
            Assert.Contains(_Driver.Title, "I am a page title - Sauce Labs");

            CleanUp(_Driver);
        }

        /// <summary>tests that the link works on the page</summary>
        [Test, Parallelizable] // denotes that this method is a test and can be run in parallel
        public void LinkWorks(string browser, string version, string platform)
        {
            // start the remote webdriver session with sauce labs
            var _Driver = _Setup(browser, version, platform);

            // find and click the link on the page
            var link = _Driver.FindElement(By.Id("i am a link"));
            link.Click();

            // wait for the page to change
            WebDriverWait wait = new WebDriverWait(_Driver, TimeSpan.FromSeconds(30));
            wait.Until((d) => { return d.Url.Contains("guinea-pig2"); });

            // verify the browser was navigated to the correct page
            Assert.Contains(_Driver.Url, "saucelabs.com/test-guinea-pig2.html");

            CleanUp(_Driver);
        }

        /// <summary>tests that a useragent element is present on the page</summary>
        [Test, Parallelizable] // denotes that this method is a test and can be run in parallel
        public void UserAgentPresent(string browser, string version, string platform)
        {
            // start the remote webdriver session with sauce labs
            var _Driver = _Setup(browser, version, platform);

            // read the useragent string off the page
            var useragent = _Driver.FindElement(By.Id("useragent")).Text;

            Assert.IsNotNull(useragent);

            CleanUp(_Driver);
        }

        #endregion
    }
}
