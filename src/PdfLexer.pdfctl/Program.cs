// See https://aka.ms/new-console-template for more information
using DotNext.Collections.Generic;
using PdfLexer;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine;
using System.Data;
using Terminal.Gui;
using PdfLexer.pdfctl.Inspect;

var rootCommand = new RootCommand();
var inspect = InspectCmd.Create();
inspect.Handler = CommandHandler.Create<InspectCmd>(InspectCmd.Handler);
rootCommand.AddCommand(inspect);
var read = ReadCmd.Create();
read.Handler = CommandHandler.Create<ReadCmd>(ReadCmd.Handler);
rootCommand.AddCommand(read);
var search = SearchCmd.Create();
search.Handler = CommandHandler.Create<SearchCmd>(SearchCmd.Handler);
rootCommand.AddCommand(search);
return await rootCommand.InvokeAsync(args);