// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.AspNet.Routing;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Test
{
    public class BodyBindingAndIntegrationTests
    {
        private class Person
        {
            [FromBody]
            [Required]
            public Address Address { get; set; }
        }

        private class Address
        {
            public string Street { get; set; }
        }

        [Fact]
        public async Task BodyBoundOnProperty_RequiredOnProperty()
        {
            // Arrange
            var argumentBinder = IntegrationTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "Parameter1",
                BindingInfo = new BindingInfo()
                {
                    BinderModelName = "CustomParameter",
                },
                ParameterType = typeof(Person)
            };

            var operationContext = IntegrationTestHelper.GetOperationBindingContext();
            var httpContext = operationContext.HttpContext;
            ConfigureHttpRequest(httpContext.Request, string.Empty);
            var modelState = new ModelStateDictionary();

            // Act
            var model = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.Equal("The Address field is required.", modelState[""].Errors.Single().ErrorMessage);
        }

        private class Person2
        {
            [FromBody]
            public Address2 Address { get; set; }
        }

        private class Address2
        {
            [Required]
            public string Street { get; set; }

            public int Zip { get; set; }
        }

        [Fact]
        public async Task BodyBoundOnProperty_RequiredOnSubProperty()
        {
            // Arrange
            var argumentBinder = IntegrationTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                BindingInfo = new BindingInfo()
                {
                    BinderModelName = "CustomParameter",
                },
                ParameterType = typeof(Person2)
            };

            var operationContext = IntegrationTestHelper.GetOperationBindingContext();
            var httpContext = operationContext.HttpContext;
            ConfigureHttpRequest(httpContext.Request, string.Empty);
            var modelState = new ModelStateDictionary();

            // Act
            var model = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.Equal("The Address field is required.", modelState[""].Errors.Single().ErrorMessage);
        }

        [Theory]
        [InlineData("{ \"Zip\" : 123 }")]
        public async Task BodyBoundOnTopLevelProperty_RequiredOnSubProperty(string inputText)
        {
            // Arrange
            var argumentBinder = IntegrationTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                BindingInfo = new BindingInfo()
                {
                    BinderModelName = "CustomParameter",
                },
                ParameterType = typeof(Person2)
            };

            var operationContext = IntegrationTestHelper.GetOperationBindingContext();
            var httpContext = operationContext.HttpContext;
            ConfigureHttpRequest(httpContext.Request, inputText);
            var modelState = new ModelStateDictionary();

            // Act
            var model = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            // TODO: this is wrong it should take the name of the property in this case.
            Assert.Equal("The Street field is required.", modelState["Address.Street"].Errors.Single().ErrorMessage);
        }


        //private class Person3
        //{
        //    public Address2 Address { get; set; }
        //}

        //private class Address2
        //{
        //    public string Street { get; set; }

        //    public int Zip { get; set; }
        //}

        //[Theory]
        //[InlineData("{ \"Zip\" : 123 }")]
        //public async Task BodyBoundOnSubProperty_RequiredOnSubSubProperty(string inputText)
        //{
        //    // Arrange
        //    var argumentBinder = IntegrationTestHelper.GetArgumentBinder();
        //    var parameter = new ParameterDescriptor()
        //    {
        //        BindingInfo = new BindingInfo()
        //        {
        //            BinderModelName = "CustomParameter",
        //        },
        //        ParameterType = typeof(Person2)
        //    };

        //    var operationContext = IntegrationTestHelper.GetOperationBindingContext();
        //    var httpContext = operationContext.HttpContext;
        //    ConfigureHttpRequest(httpContext.Request, inputText);
        //    var modelState = new ModelStateDictionary();

        //    // Act
        //    var model = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

        //    // Assert
        //    // TODO: this is wrong it should take the name of the property in this case.
        //    Assert.Equal("The Street field is required.", modelState["Address.Street"].Errors.Single().ErrorMessage);
        //}

        public static void ConfigureHttpRequest(HttpRequest request, string jsonContent)
        {
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
            request.ContentType = "application/json";
        }
    }
}