﻿using Newtonsoft.Json;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using SSSISE = Sucrose.Shared.Space.Interface.SerializableException;
using SSSISFD = Sucrose.Shared.Space.Interface.StackFrameData;

namespace Sucrose.Shared.Space.Converter
{
    public class ExceptionConverter : JsonConverter<Exception>
    {
        public override void WriteJson(JsonWriter writer, Exception value, JsonSerializer serializer)
        {
            SSSISE serializableException = ConvertToSerializableException(value);
            serializer.Serialize(writer, serializableException);
        }

        private SSSISE ConvertToSerializableException(Exception exception)
        {
            if (exception == null)
            {
                return null;
            }

            return new SSSISE
            {
                Data = exception.Data,
                Source = exception.Source,
                Message = exception.Message,
                HResult = exception.HResult,
                HelpURL = exception.HelpLink,
                StackTrace = ParseStackTrace(exception),
                ClassName = exception.GetType().FullName,
                InnerException = ConvertToSerializableException(exception.InnerException)
            };
        }

        private List<SSSISFD> ParseStackTrace(Exception exception)
        {
            StackTrace stackTrace = new(exception, true);
            StackFrame[] frames = stackTrace.GetFrames();

            if (frames == null)
            {
                return null;
            }

            List<SSSISFD> frameList = new();

            foreach (StackFrame frame in frames)
            {
                frameList.Add(new SSSISFD
                {
                    FileName = frame.GetFileName(),
                    Method = frame.GetMethod().ToString(),
                    LineNumber = frame.GetFileLineNumber(),
                    ColumnNumber = frame.GetFileColumnNumber()
                });
            }

            return frameList;
        }

        public override Exception ReadJson(JsonReader reader, Type objectType, Exception existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            SSSISE serializableException = serializer.Deserialize<SSSISE>(reader);
            return ConvertToException(serializableException);
        }

        private Exception ConvertToException(SSSISE serializableException)
        {
            if (serializableException == null)
            {
                return null;
            }

            Exception exception = (Exception)Activator.CreateInstance(Type.GetType(serializableException.ClassName), serializableException.Message);
            exception.HelpLink = serializableException.HelpURL;

            foreach (DictionaryEntry entry in serializableException.Data)
            {
                exception.Data.Add(entry.Key, entry.Value);
            }

            if (serializableException.InnerException != null)
            {
                Exception innerException = ConvertToException(serializableException.InnerException);
                FieldInfo field = typeof(Exception).GetField("_innerException", BindingFlags.NonPublic | BindingFlags.Instance);
                field.SetValue(exception, innerException);
            }

            return exception;
        }
    }
}