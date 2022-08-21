using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using pdflexer.TestCaseGen;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;

await BuildCommandLine()
            .UseHost(_ => Host.CreateDefaultBuilder(),
                host =>
                {
                    host.ConfigureServices((ctx, sc) =>
                    {
                        sc
                            .AddSingleton(ctx.Configuration)
                            .AddTransient<ImageSampler>()
                            .AddTransient<TextSampler>()
                            .AddLogging(x => x.AddConsole());
                    });
                })
            .UseDefaults()
            .Build()
            .InvokeAsync(args);


static CommandLineBuilder BuildCommandLine()
{
    var root = new RootCommand();
    var run = RunImageCmd.CreateCommand();
    run.Handler = CommandHandler.Create<RunImageCmd, IHost>(RunImages);
    root.AddCommand(run);
    var gen = GenStdGlyph.CreateCommand();
    gen.Handler = CommandHandler.Create<GenStdGlyph, IHost>(RunGen);
    root.AddCommand(gen);

    var txt = RunTextCmd.CreateCommand();
    txt.Handler = CommandHandler.Create<RunTextCmd, IHost>(RunText);
    root.AddCommand(txt);

    return new CommandLineBuilder(root);

}

static Task RunImages(RunImageCmd cmd, IHost host)
{
    var provider = host.Services;
    var proc = provider.GetRequiredService<ImageSampler>();
    proc.Run(cmd);
    return Task.CompletedTask;
}

static Task RunText(RunTextCmd cmd, IHost host)
{
    var provider = host.Services;
    var proc = provider.GetRequiredService<TextSampler>();
    proc.Run(cmd);
    return Task.CompletedTask;
}

static Task RunGen(GenStdGlyph cmd, IHost host)
{
    GenStdGlyph.Run();
    return Task.CompletedTask;
}
