﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JsonPatchWebSite;
using JsonPatchWebSite.Models;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class JsonPatchTest
    {
        private const string SiteName = nameof(JsonPatchWebSite);
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;
        private readonly Action<IServiceCollection> _configureServices = new Startup().ConfigureServices;

        [Theory]
        [InlineData("http://localhost/JsonPatch/JsonPatchWithModelState")]
        [InlineData("http://localhost/JsonPatch/JsonPatchWithModelStateAndPrefix?prefix=Patch")]
        public async Task JsonPatch_ValidAddOperation_List(string url)
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            var input = "[{ \"op\": \"add\", \"path\": \"Customer/Orders/2\", " +
               "\"value\": { \"OrderName\": \"Name2\" }}]";
            var request = new HttpRequestMessage
            {
                Content = new StringContent(input, Encoding.UTF8, "application/json-patch+json"),
                Method = new HttpMethod("PATCH"),
                RequestUri = new Uri(url)
            };

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<Customer>(body);
            Assert.Equal("Name2", customer.Orders[2].OrderName);
        }


        [Theory]
        [InlineData("http://localhost/JsonPatch/JsonPatchWithModelState")]
        [InlineData("http://localhost/JsonPatch/JsonPatchWithModelStateAndPrefix?prefix=Patch")]
        public async Task JsonPatch_MultipleValidOperations_Success(string url)
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            var input = "[{ \"op\": \"add\", \"path\": \"Customer/Orders/2\", " +
               "\"value\": { \"OrderName\": \"Name2\" }}, {\"op\": \"copy\", \"from\": \"Customer/Orders/2\", " +
                "\"path\": \"Customer/Orders/3\" }, {\"op\": \"replace\", \"path\": \"Customer/Orders/2/OrderName\", " +
                "\"value\": \"ReplacedName\" }]";
            var request = new HttpRequestMessage
            {
                Content = new StringContent(input, Encoding.UTF8, "application/json-patch+json"),
                Method = new HttpMethod("PATCH"),
                RequestUri = new Uri(url)
            };

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<Customer>(body);
            Assert.Equal("ReplacedName", customer.Orders[2].OrderName);
            Assert.Equal("Name2", customer.Orders[3].OrderName);
        }

        public static IEnumerable<object[]> InvalidJsonPatchData
        {
            get
            {
                return new[]
                {
                    new object[] {
                        "http://localhost/JsonPatch/JsonPatchWithModelStateAndPrefix?prefix=Patch",
                        "[{ \"op\": \"add\", \"path\": \"Customer/Orders/5\", " +
                        "\"value\": { \"OrderName\": \"Name5\" }}]",
                        "{\"Patch.JsonPatchDocument`1\":[\"For operation 'add' on array property at path " +
                        "'Customer/Orders/5', the index is larger than the array size.\"]}"
                    },
                    new object[] {
                        "http://localhost/JsonPatch/JsonPatchWithModelState",
                        "[{ \"op\": \"add\", \"path\": \"Customer/Orders/5\", " +
                        "\"value\": { \"OrderName\": \"Name5\" }}]",
                        "{\"JsonPatchDocument`1\":[\"For operation 'add' on array property at path " +
                        "'Customer/Orders/5', the index is larger than the array size.\"]}"
                    },
                    new object[] {
                        "http://localhost/JsonPatch/JsonPatchWithModelStateAndPrefix?prefix=Patch",
                        "[{ \"op\": \"add\", \"path\": \"Customer/Orders/2\", \"value\": " +
                        "{ \"OrderName\": \"Name2\" }}, {\"op\": \"copy\", \"from\": \"Customer/Orders/4\", " +
                        "\"path\": \"Customer/Orders/3\" }, {\"op\": \"replace\", \"path\": " +
                        "\"Customer/Orders/2/OrderName\", \"value\": \"ReplacedName\" }]",
                        "{\"Patch.JsonPatchDocument`1\":[\"For operation 'copy' on array property at path " +
                        "'Customer/Orders/4', the index is larger than the array size.\"]}"
                    },
                    new object[] {
                        "http://localhost/JsonPatch/JsonPatchWithModelState",
                        "[{ \"op\": \"add\", \"path\": \"Customer/Orders/2\", \"value\": " +
                        "{ \"OrderName\": \"Name2\" }}, {\"op\": \"copy\", \"from\": \"Customer/Orders/4\", " +
                        "\"path\": \"Customer/Orders/3\" }, {\"op\": \"replace\", \"path\": " +
                        "\"Customer/Orders/2/OrderName\", \"value\": \"ReplacedName\" }]",
                        "{\"JsonPatchDocument`1\":[\"For operation 'copy' on array property at path " +
                        "'Customer/Orders/4', the index is larger than the array size.\"]}"
                    }
                };
            }
        }

        [Theory, MemberData("InvalidJsonPatchData")]
        public async Task JsonPatch_InvalidOperations_failure(string url, string input, string errorMessage)
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            var request = new HttpRequestMessage
            {
                Content = new StringContent(input, Encoding.UTF8, "application/json-patch+json"),
                Method = new HttpMethod("PATCH"),
                RequestUri = new Uri(url)
            };

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(errorMessage, body);
        }
    }
}