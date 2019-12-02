﻿using System;
using System.IO;
using System.Linq;
using Cottle.Documents.Dynamic;
using Cottle.Exceptions;
using Cottle.Settings;
using Cottle.Stores;

namespace Cottle.Documents
{
    /// <summary>
    ///     Dynamic document compiles template using MSIL generation for better
    ///     performance. Code generated by JIT compiler can be reclaimed by garbage
    ///     collector, but you should use a caching mechanism to avoid re-creating
    ///     too many DynamicDocument instances using the same template source.
    /// </summary>
    public sealed class DynamicDocument : AbstractDocument
    {
        private readonly DynamicFunction _root;

        [Obsolete("Use `Document.CreateNative(template, configuration).DocumentOrThrow` to get an equivalent document instance")]
        public DynamicDocument(TextReader reader, ISetting setting)
        {
            var parser = ParserFactory.BuildParser(AbstractDocument.CreateConfiguration(setting));

            if (!parser.Parse(reader, out var command, out var reports))
            {
                var firstReport = reports.Count > 0 ? reports[0] : new DocumentReport("unknown error", 0, 0);

                throw new ParseException(firstReport.Column, firstReport.Line, firstReport.Message);
            }

            _root = new DynamicFunction(Enumerable.Empty<string>(), command);
        }

        [Obsolete("Use `Document.CreateNative(template).DocumentOrThrow` to get an equivalent document instance")]
        public DynamicDocument(TextReader reader) :
            this(reader, DefaultSetting.Instance)
        {
        }

        [Obsolete("Use `Document.CreateNative(template, configuration).DocumentOrThrow` to get an equivalent document instance")]
        public DynamicDocument(string template, ISetting setting) :
            this(new StringReader(template), setting)
        {
        }

        [Obsolete("Use `Document.CreateNative(template).DocumentOrThrow` to get an equivalent document instance")]
        public DynamicDocument(string template) :
            this(new StringReader(template), DefaultSetting.Instance)
        {
        }

        public override Value Render(IContext context, TextWriter writer)
        {
            return _root.Invoke(new ContextStore(context), Array.Empty<Value>(), writer);
        }

        public static void Save(TextReader reader, ISetting setting, string assemblyName, string fileName)
        {
            var parser = ParserFactory.BuildParser(AbstractDocument.CreateConfiguration(setting));

            if (!parser.Parse(reader, out var command, out var reports))
            {
                var firstReport = reports.Count > 0 ? reports[0] : new DocumentReport("unknown error", 0, 0);

                throw new ParseException(firstReport.Column, firstReport.Line, firstReport.Message);
            }

            DynamicFunction.Save(command, setting.Trimmer, assemblyName, fileName);
        }
    }
}