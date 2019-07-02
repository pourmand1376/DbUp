using System;
using System.Collections.Generic;
using System.Linq;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Engine.Transactions;
using DbUp.Support;
using DbUp.Tests.TestInfrastructure;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace DbUp.Tests
{
    [Story(
         AsA = "As a DbUp User",
         IWant = "I want to DbUp to upgrade my database to the latest version with a run always script",
         SoThat = "So that run always scripts always run and the run once script only run once")]
    public class RunOnChangeScriptTypeScenarios
    {
        readonly List<SqlScript> scripts;
        readonly UpgradeEngineBuilder upgradeEngineBuilder;
        readonly CaptureLogsLogger logger;
        readonly DelegateConnectionFactory testConnectionFactory;
        readonly RecordingDbConnection recordingConnection;
        DatabaseUpgradeResult upgradeResult;
        UpgradeEngine upgradeEngine;
        bool isUpgradeRequired;

        public RunOnChangeScriptTypeScenarios()
        {
            upgradeResult = null;
            
            scripts = new List<SqlScript>
            {
               new SqlScript("Script1.sql", "Create or Alter Procedure abc", new SqlScriptOptions { ScriptType = ScriptType.RunOnChange}),
            };

            logger = new CaptureLogsLogger();
            recordingConnection = new RecordingDbConnection(logger, "SchemaVersions");
            testConnectionFactory = new DelegateConnectionFactory(_ => recordingConnection);

            upgradeEngineBuilder = DeployChanges.To
                .SqlDatabase("testconn")
                .WithScripts(new TestScriptProvider(scripts))
                .OverrideConnectionFactory(testConnectionFactory)
                .LogTo(logger);
        }

     
        [Fact]
        public void UpgradingAnOutOfDateDatabase()
        {
            this.Given(t => t.GivenAnOutOfDateDatabase())
                .When(t => t.WhenDatabaseIsUpgraded())
                .Then(t => t.ThenShouldHaveSuccessfulResult())
                .And(t => t.AndShouldHaveRunUpgradeScript())
                .And(t => t.AndShouldLogInformation())
                .BDDfy();
        }

        [Fact]
        public void AttemptingToUpgradeAnUptoDateDatabase()
        {
            this.Given(t => t.GivenAnUpToDateDatabase())
                .When(t => t.WhenDatabaseIsUpgraded())
                .Then(t => t.ThenShouldHaveNotRunAnyScripts())
                .And(t => t.ThenShouldHaveSuccessfulResult())
                .BDDfy();
        }

        void AndShouldLogInformation()
        {
            logger.InfoMessages.ShouldContain("Beginning database upgrade");
            logger.InfoMessages.ShouldContain("Upgrade successful");
        }

        void AndShouldHaveRunUpgradeScript()
        {
            // Check both results and journal
            upgradeResult.Scripts
                .Select(s => s.Name)
                .ShouldBe(new[] {"Script1.sql"});

            upgradeResult.Scripts
                .Select(s => s.Contents)
                .ShouldBe(new[] { "Create or Alter Procedure abc" });
        }

        void ThenShouldHaveNotRunAnyScripts()
        {
            upgradeResult.Scripts.Select(s => s.Name).ShouldBe(new String[] {});
        }

        void ThenShouldHaveSuccessfulResult()
        {
            upgradeResult.Successful.ShouldBeTrue();
        }

        void GivenAnOutOfDateDatabase()
        {
            DateTime runAt = DateTime.Parse("01/01/1983");
            SqlScript sqlScript1 = new SqlScript("Script1.sql", "Create or Alter Procedure abc", runAt, new SqlScriptOptions { ScriptType = ScriptType.RunOnChange });
            SqlScript sqlScript2 = new SqlScript("Script1.sql", "Create or Alter Procedure xyz", runAt.AddSeconds(1), new SqlScriptOptions { ScriptType = ScriptType.RunOnChange });
            recordingConnection.SetupRunScripts(sqlScript1, sqlScript2);
        }

        void GivenAnUpToDateDatabase()
        {
            DateTime runAt = DateTime.Parse("01/01/1983");
            SqlScript sqlScript1 = new SqlScript("Script1.sql", "Create or Alter Procedure abc", runAt, new SqlScriptOptions { ScriptType = ScriptType.RunOnChange });
            SqlScript sqlScript2 = new SqlScript("Script1.sql", "Create or Alter Procedure xyz", runAt.AddSeconds(1), new SqlScriptOptions { ScriptType = ScriptType.RunOnChange });
            SqlScript sqlScript3 = new SqlScript("Script1.sql", "Create or Alter Procedure abc", runAt.AddSeconds(2), new SqlScriptOptions { ScriptType = ScriptType.RunOnChange });
            recordingConnection.SetupRunScripts(sqlScript1);
        }

        void WhenCheckIfDatabaseUpgradeIsRequired()
        {
            upgradeEngine = upgradeEngineBuilder.Build();
            isUpgradeRequired = upgradeEngine.IsUpgradeRequired();
        }

        void WhenDatabaseIsUpgraded()
        {
            upgradeEngine = upgradeEngineBuilder.Build();
            upgradeResult = upgradeEngine.PerformUpgrade();
        }

        void ThenUpgradeShouldNotBeRequired()
        {
            isUpgradeRequired.ShouldBeFalse();
        }

        void ThenUpgradeShouldBeRequired()
        {
            isUpgradeRequired.ShouldBeTrue();
        }

        public class TestScriptProvider : IScriptProvider
        {
            readonly List<SqlScript> sqlScripts;

            public TestScriptProvider(List<SqlScript> sqlScripts)
            {
                this.sqlScripts = sqlScripts;
            }

            public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
            {
                return sqlScripts;
            }
        }
    }
}