using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace blqw.Unitable
{
    /// <summary>
    /// 日志组件
    /// </summary>
    internal static class Logger
    {
        /// <summary>
        /// TraceSource
        /// </summary>
        private static TraceSource Trace { get; } = new TraceSource("Haibei.Common", SourceLevels.Verbose).Initialize();

        static Logger() => DebuggerIfAttached(Trace.Listeners);

        /// <summary>
        /// 同步 <seealso cref="Trace.Listeners" /> 和 <seealso cref="LoggerSource.Listeners" /> 中的对象
        /// 如果无法同步,则进行复制
        /// </summary>
        private static void SyncTraceListeners(TraceSource source)
        {
            //如果只有一个监听器 且是默认的监听器,则同步 Trace.Listeners
            if ((source.Listeners.Count != 1) || (source.Listeners[0] is DefaultTraceListener == false))
            {
                return;
            }
            var field = typeof(TraceSource).GetField("listeners", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?? typeof(TraceSource)
                            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                            .FirstOrDefault(it => it.FieldType == typeof(TraceListenerCollection));
            if (field?.IsLiteral == false)
            {
                try
                {
                    field.SetValue(source, Trace.Listeners);
                    return;
                }
                catch
                {
                    // ignored
                }
            }

            source.Listeners.Clear();
            source.Listeners.AddRange(Trace.Listeners);
        }

        /// <summary>
        /// 如果处于调试器附加进程状态,则追加调试日志侦听器
        /// </summary>
        private static void DebuggerIfAttached(TraceListenerCollection listeners)
        {
            //非调试模式
            if (Debugger.IsAttached == false)
            {
                return;
            }

            if (listeners.OfType<DefaultTraceListener>().Any() == false)
            {
                //可以在"输出"窗口看到所有的日志
                listeners.Add(new DefaultTraceListener());
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="source"> </param>
        /// <returns> </returns>
        public static TraceSource Initialize(this TraceSource source)
        {
            SyncTraceListeners(source);
            DebuggerIfAttached(source.Listeners);
            return source;
        }


        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="_source"> </param>
        /// <param name="eventType"> 日志类型 </param>
        /// <param name="message"> 日志消息 </param>
        /// <param name="ex"> 异常对象 </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="message" /> 为 null 。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="message" /> 为空字符串 ("")或连续的空白。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public static void Write(TraceEventType eventType, string message, Exception ex = null,
            [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException($"{nameof(message)}为空字符串 (\"\")或连续的空白。", nameof(message));
            }
            if (ex == null)
            {
                Trace.TraceData(eventType, 0, message);
            }
            else
            {
                Trace.TraceData(eventType, 0, message, $"{file}:{line},{member}", ex.ToString());
            }
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="_source"> </param>
        /// <param name="eventType"> 日志类型 </param>
        /// <param name="getMessage"> 用户获取日志的委托 </param>
        /// <param name="ex"> </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="getMessage" /> 为 null。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="getMessage" /> 返回值为null或连续的空白。 </exception>
        /// <exception cref="Exception"> <paramref name="getMessage" /> 执行出现异常。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public static void Write(TraceEventType eventType, Func<string> getMessage, Exception ex = null,
            [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
        {
            if (getMessage == null)
            {
                throw new ArgumentNullException(nameof(getMessage));
            }

            if (Trace.Switch.ShouldTrace(eventType) == false)
            {
                return;
            }

            var message = getMessage();
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException($"{nameof(getMessage)}返回值为null或连续的空白。", nameof(getMessage));
            }

            if (ex == null)
            {
                Trace.TraceData(eventType, 0, message);
            }
            else
            {
                Trace.TraceData(eventType, 0, message, $"{file}:{line},{member}", ex.ToString());
            }
        }

        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="message"> 日志消息 </param>
        /// <param name="ex"> 异常对象 </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="message" /> 为 null 。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="message" /> 为空字符串 ("")或连续的空白。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public static void Debug(string message, Exception ex = null,
            [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
            => Write(TraceEventType.Verbose, message, ex, line, member, file);

        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="getMessage"> 用户获取日志的委托 </param>
        /// <param name="ex"> </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="getMessage" /> 为 null。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="getMessage" /> 返回值为null或连续的空白。 </exception>
        /// <exception cref="Exception"> <paramref name="getMessage" /> 执行出现异常。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public static void Debug(Func<string> getMessage, Exception ex = null,
            [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
            => Write(TraceEventType.Verbose, getMessage, ex, line, member, file);

        /// <summary>
        /// 提示日志
        /// </summary>
        /// <param name="message"> 日志消息 </param>
        /// <param name="ex"> 异常对象 </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="message" /> 为 null 。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="message" /> 为空字符串 ("")或连续的空白。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public static void Information(string message, Exception ex = null,
            [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
            => Write(TraceEventType.Information, message, ex, line, member, file);

        /// <summary>
        /// 提示日志
        /// </summary>
        /// <param name="getMessage"> 用户获取日志的委托 </param>
        /// <param name="ex"> </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="getMessage" /> 为 null。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="getMessage" /> 返回值为null或连续的空白。 </exception>
        /// <exception cref="Exception"> <paramref name="getMessage" /> 执行出现异常。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public static void Information(Func<string> getMessage, Exception ex = null,
            [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
            => Write(TraceEventType.Information, getMessage, ex, line, member, file);

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="message"> 日志消息 </param>
        /// <param name="ex"> 异常对象 </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="message" /> 为 null 。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="message" /> 为空字符串 ("")或连续的空白。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public static void Warning(string message, Exception ex = null,
            [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
            => Write(TraceEventType.Warning, message, ex, line, member, file);

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="getMessage"> 用户获取日志的委托 </param>
        /// <param name="ex"> </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="getMessage" /> 为 null。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="getMessage" /> 返回值为null或连续的空白。 </exception>
        /// <exception cref="Exception"> <paramref name="getMessage" /> 执行出现异常。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public static void Warning(Func<string> getMessage, Exception ex = null,
            [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
            => Write(TraceEventType.Warning, getMessage, ex, line, member, file);

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="message"> 日志消息 </param>
        /// <param name="ex"> 异常对象 </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="message" /> 为 null 。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="message" /> 为空字符串 ("")或连续的空白。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public static void Error(string message, Exception ex = null,
            [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
            => Write(TraceEventType.Error, message, ex, line, member, file);

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="getMessage"> 用户获取日志的委托 </param>
        /// <param name="ex"> </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="getMessage" /> 为 null。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="getMessage" /> 返回值为null或连续的空白。 </exception>
        /// <exception cref="Exception"> <paramref name="getMessage" /> 执行出现异常。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public static void Error(Func<string> getMessage, Exception ex = null,
            [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
            => Write(TraceEventType.Error, getMessage, ex, line, member, file);

        /// <summary>
        /// 崩溃日志
        /// </summary>
        /// <param name="message"> 日志消息 </param>
        /// <param name="ex"> 异常对象 </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="message" /> 为 null 。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="message" /> 为空字符串 ("")或连续的空白。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public static void Critical(string message, Exception ex = null,
            [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
            => Write(TraceEventType.Critical, message, ex, line, member, file);

        /// <summary>
        /// 崩溃日志
        /// </summary>
        /// <param name="getMessage"> 用户获取日志的委托 </param>
        /// <param name="ex"> </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="getMessage" /> 为 null。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="getMessage" /> 返回值为null或连续的空白。 </exception>
        /// <exception cref="Exception"> <paramref name="getMessage" /> 执行出现异常。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public static void Critical(Func<string> getMessage, Exception ex = null,
            [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
            => Write(TraceEventType.Critical, getMessage, ex, line, member, file);

        /// <summary>
        /// 进入方法
        /// </summary>
        public static void Entry([CallerLineNumber] int line = 0, [CallerMemberName] string member = null, [CallerFilePath] string file = null)
            => Write(TraceEventType.Start, $"进入方法 {member}", null, line, member, file);

        /// <summary>
        /// 离开方法并有一个返回值
        /// </summary>
        public static void Return(string @return, [CallerLineNumber] int line = 0, [CallerMemberName] string member = null, [CallerFilePath] string file = null)
            => Write(TraceEventType.Stop, $"离开方法 {member}, 结果: {@return ?? "<null>"}", null, line, member, file);

        /// <summary>
        /// 离开方法
        /// </summary>
        public static void Exit(this TraceSource source, [CallerLineNumber] int line = 0, [CallerMemberName] string member = null, [CallerFilePath] string file = null)
            => Write(TraceEventType.Stop, $"离开方法 {member}", null, line, member, file);


    }
}
