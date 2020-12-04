using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.Archive;
using Service.Configuration;
using Service.Enums;
using Service.ErrorReport;
using Service.Interfaces;
using Service.Messaging;

namespace Service
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        ArchiveProcessorConfig Config { get; }

        public Startup()
        {
            Config = new ArchiveProcessorConfig();

            var builder = new ConfigurationBuilder()
                .AddJsonFile("config/appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Bind(Config);

            if (string.IsNullOrEmpty(Config.MessageBrokerUser))
            {
                Config.MessageBrokerUser = "guest";
            }

            if (string.IsNullOrEmpty(Config.MessageBrokerPassword))
            {
                Config.MessageBrokerPassword = "guest";
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole());
            services.AddScoped<IAdaptationOutcomeSender, AdaptationOutcomeSender>();
            services.AddScoped<IAdaptationRequestSender, AdaptationRequestSender>();
            services.AddScoped<IResponseProcessor, AdaptationOutcomeProcessor>();
            services.AddTransient<IArchiveProcessor, ArchiveProcessor>();
            services.AddTransient<IFileManager, LocalFileManager>();
            services.AddScoped<IAdaptationResponseCollection, AdaptationResponseCollection>();
            services.AddTransient<IAdaptationResponseConsumer, AdaptationResponseConsumer>();
            services.AddScoped<IAdaptationResponseProducer, AdaptationResponseProducer>();
            services.AddTransient<IErrorReportGenerator, HtmlErrorReportGenerator>();
            services.AddTransient<IPasswordProtectedReportGenerator, HtmlPasswordProtectedErrorReportGenerator>();
            services.AddSingleton<IArchiveProcessorConfig>(Config);

            services.AddTransient<ZipArchiveManager>();
            services.AddTransient<TarArchiveManager>();
            services.AddTransient<RarArchiveManager>();
            services.AddTransient<SevenZipArchiveManager>();
            services.AddTransient<GZipArchiveManager>();

            services.AddTransient<IArchiveManager>(s =>
            {
                FileType fileType = (FileType)Enum.Parse(typeof(FileType), Config.ArchiveFileType);
                switch (fileType) 
                {
                    case (FileType.Zip):
                        return s.GetService<ZipArchiveManager>();
                    case (FileType.Tar):
                        return s.GetService<TarArchiveManager>();
                    case (FileType.Rar):
                        return s.GetService<RarArchiveManager>();
                    case (FileType.SevenZip):
                        return s.GetService<SevenZipArchiveManager>();
                    case (FileType.Gzip):
                        return s.GetService<GZipArchiveManager>();
                    default:
                        throw new NotImplementedException();
                }
            });
        }
    }
}