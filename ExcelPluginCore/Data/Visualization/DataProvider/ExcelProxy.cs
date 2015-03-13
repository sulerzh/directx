using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Microsoft.Data.Visualization.DataProvider
{
    public class ExcelProxy<T>
    {
        private const int s_EnglishCultureID = 1033;
        private static readonly CultureInfo s_EnglishCulture = CultureInfo.GetCultureInfo(s_EnglishCultureID);
        private T m_Interface;

        public ExcelProxy(T excelInterface)
        {
            if (excelInterface == null)
                throw new ArgumentNullException("excelInterface");
            m_Interface = excelInterface;
        }

        public static explicit operator T(ExcelProxy<T> proxy)
        {
            return proxy.m_Interface;
        }

        public void Invoke(Action<T> action)
        {
            SafeRun(action);
        }

        public TResult Invoke<TResult>(Func<T, TResult> action)
        {
            TResult result = default(TResult);
            SafeRun(item => result = action(item));
            return result;
        }

        public bool TryInvoke(Action<T> action)
        {
            try
            {
                Invoke(action);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public ExcelProxy<TResult> InvokeAndProxy<TResult>(Func<T, TResult> action)
        {
            TResult result = default(TResult);
            SafeRun(item => result = action(item));
            return new ExcelProxy<TResult>(result);
        }

        public ExcelProxy<TResult> InvokeAndProxyOptional<TResult>(Func<T, TResult> action)
        {
            TResult result = default(TResult);
            SafeRun(item => result = action(item));
            if (result == null)
                return null;
            return new ExcelProxy<TResult>(result);
        }

        public IEnumerable<ExcelProxy<TResult>> Enumerate<TResult>(Func<T, IEnumerable> enumerable)
        {
            List<ExcelProxy<TResult>> results = null;
            SafeRun(item =>
            {
                results = new List<ExcelProxy<TResult>>();
                foreach (TResult excelInterface in enumerable(item))
                    results.Add(new ExcelProxy<TResult>(excelInterface));
            });
            return results;
        }

        private void SafeRun(Action<T> action)
        {
            Thread currentThread = Thread.CurrentThread;
            CultureInfo currentCulture = currentThread.CurrentCulture;
            try
            {
                currentThread.CurrentCulture = s_EnglishCulture;
                action(m_Interface);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
            }
        }
    }
}
