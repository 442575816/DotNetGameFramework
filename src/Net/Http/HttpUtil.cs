using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace DotNetGameFramework
{
    public static class HttpUtil
    {
        /// <summary>
        /// URL正则表达式
        /// </summary>
        private static readonly Regex URL_REGEX = new Regex("^/root/([\\w-/]+).action");

        /// <summary>
        /// 获取Command
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetCommand(Microsoft.AspNetCore.Http.HttpRequest httpRequest)
        {
            return GetCommand(httpRequest.Path);
        }

        /// <summary>
        /// 获取Command
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetCommand(string uriPath)
        {
            var match = URL_REGEX.Match(uriPath);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }

        /// <summary>
        /// 处理Http响应
        /// </summary>
        /// <param name="http"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task HandleHttpResponse(HttpContext http, HttpRequest request, HttpResponse response)
        {
            var httpResponse = http.Response;

            // 设置Header
            var headers = response.Headers;
            if (headers != null)
            {
                foreach(var item in headers)
                {
                    httpResponse.Headers.Add(item.Key, item.Value);
                }
            }

            // 设置Cookie
            var cookies = response.Cookies;
            if (cookies != null)
            {
                foreach (var item in cookies)
                {
                    httpResponse.Cookies.Append(item.Key, item.Value);
                }
            }

            // 设置响应吗
            httpResponse.StatusCode = response.HttpStatus;

            // HEAD请求没有响应体
            if (http.Request.Method == HttpMethods.Head)
            {
                return;
            }

            // 处理响应
            var buff = response.Content as ByteBuf;
            if (null != buff)
            {
                httpResponse.ContentLength = buff.ReadableBytes;
                await httpResponse.Body.WriteAsync(buff.Data, buff.ReaderIndex, buff.ReadableBytes);
                buff.Release();
            }
        }

        /// <summary>
        /// 获取Cookie值
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetCookieValue(Microsoft.AspNetCore.Http.HttpRequest httpRequest, string key)
        {
            var cookies = httpRequest.Cookies;
            cookies.TryGetValue(key, out string value);

            return value;
        }

        /// <summary>
        /// 填充参数
        /// </summary>
        /// <param name="parameterMap"></param>
        /// <param name="item"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillParameter(Dictionary<string, string[]> parameterMap, KeyValuePair<string, StringValues> item)
        {
            if (item.Value.Count == 0)
            {
                return;
            }

            if (parameterMap.TryGetValue(item.Key, out string[] values))
            {
                string[] newArray = new string[values.Length + item.Value.Count];
                Array.Copy(values, newArray, values.Length);
                Array.Copy(item.Value, values.Length, newArray, 0, item.Value.Count);

                parameterMap[item.Key] = newArray;
            }
            else
            {
                parameterMap[item.Key] = item.Value.ToArray();
            }
        }

        /// <summary>
        /// 填充参数
        /// </summary>
        /// <param name="parameterMap"></param>
        /// <param name="item"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillParameter(Dictionary<string, string[]> parameterMap, KeyValuePair<string, string> item)
        {
            if (item.Value != null)
            {
                return;
            }

            if (parameterMap.TryGetValue(item.Key, out string[] values))
            {
                string[] newArray = new string[values.Length + 1];
                Array.Copy(values, newArray, values.Length);
                newArray[values.Length] = item.Value;

                parameterMap[item.Key] = newArray;
            }
            else
            {
                parameterMap[item.Key] = new string[] { item.Value };
            }
        }

        /// <summary>
        /// 获取HeaderValue
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetHeaderValue(Microsoft.AspNetCore.Http.HttpRequest httpRequest, string key)
        {
            var headers = httpRequest.Headers;
            headers.TryGetValue(key, out var values);

            if (values.Count > 0)
            {
                return values[0];
            }

            return null;
        }
    }
}
