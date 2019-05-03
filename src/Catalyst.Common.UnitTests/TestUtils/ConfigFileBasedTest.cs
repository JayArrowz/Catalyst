#region LICENSE

/**
* Copyright (c) 2019 Catalyst Network
*
* This file is part of Catalyst.Node <https://github.com/catalyst-network/Catalyst.Node>
*
* Catalyst.Node is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 2 of the License, or
* (at your option) any later version.
*
* Catalyst.Node is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with Catalyst.Node. If not, see <https://www.gnu.org/licenses/>.
*/

#endregion

using System.IO;
using Autofac;
using Autofac.Configuration;
using AutofacSerilogIntegration;
using Catalyst.Common.Interfaces.Cryptography;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using SharpRepository.Ioc.Autofac;
using SharpRepository.Repository;
using Xunit.Abstractions;

namespace Catalyst.Common.UnitTests.TestUtils
{
    public class ConfigFileBasedTest : FileSystemBasedTest
    {
        protected ContainerBuilder ContainerBuilder;
        protected ConfigFileBasedTest(ITestOutputHelper output) : base(output) { }

        protected string LogOutputTemplate { get; set; } =
            "{Timestamp:HH:mm:ss} [{Level:u3}] ({ThreadId}) {Message} ({SourceContext}){NewLine}{Exception}";

        protected LogEventLevel LogEventLevel { get; set; } = LogEventLevel.Verbose;

        protected virtual void ConfigureContainerBuilder(IConfigurationRoot config,
            bool writeLogsToTestOutput = false,
            bool writeLogsToFile = false)
        {
            var configurationModule = new ConfigurationModule(config);
            ContainerBuilder = new ContainerBuilder();
            ContainerBuilder.RegisterModule(configurationModule);
            ContainerBuilder.RegisterInstance(config).As<IConfigurationRoot>();

            var repoFactory =
                RepositoryFactory.BuildSharpRepositoryConfiguation(config.GetSection("PersistenceConfiguration"));
            ContainerBuilder.RegisterSharpRepository(repoFactory);

            var passwordReader = new TestPasswordReader();
            ContainerBuilder.RegisterInstance(passwordReader).As<IPasswordReader>();

            var certificateStore = new TestCertificateStore();
            ContainerBuilder.RegisterInstance(certificateStore).As<ICertificateStore>();

            ConfigureLogging(config, writeLogsToTestOutput, writeLogsToFile);
        }

        private void ConfigureLogging(IConfigurationRoot config, bool writeLogsToTestOutput, bool writeLogsToFile)

        {
            var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(config).MinimumLevel.Verbose();

            if (writeLogsToTestOutput)
            {
                loggerConfiguration.WriteTo.TestOutput(Output, LogEventLevel, LogOutputTemplate);
            }

            if (writeLogsToFile)
            {
                loggerConfiguration.WriteTo.File(Path.Combine(FileSystem.GetCatalystHomeDir().FullName, "Catalyst.Node.log"), LogEventLevel,
                    LogOutputTemplate);
            }

            ContainerBuilder.RegisterLogger(loggerConfiguration.CreateLogger());
        }
    }
}