// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.ModelBinding.Test
{
    public static class IntegrationTestHelper
    {
        public static OperationBindingContext GetOperationBindingContext()
        {
            return new OperationBindingContext()
            {
                BodyBindingState = BodyBindingState.NotBodyBased,
                HttpContext = IntegrationTestHelper.GetHttpContext(),
                MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                ValidatorProvider = TestModelValidatorProvider.CreateDefaultProvider(),
                ValueProvider = new TestValueProvider(new Dictionary<string, object>()),
                ModelBinder = TestModelBinderProvider.CreateDefaultModelBinder()
            };
        }

        public static DefaultControllerActionArgumentBinder GetArgumentBinder()
        {
            var options = new TestMvcOptions();
            options.Options.MaxModelValidationErrors = 5;
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            return new DefaultControllerActionArgumentBinder(
                metadataProvider,
                new DefaultObjectValidator(
                    options.Options.ValidationExcludeFilters,
                    metadataProvider),
                options);
        }

        public static HttpContext GetHttpContext()
        {
            var options = (new TestMvcOptions()).Options;
            var httpContext = new DefaultHttpContext();
            var serviceCollection = MvcServices.GetDefaultServices();
            httpContext.RequestServices = serviceCollection.BuildServiceProvider();

            var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());

            var actionContextAccessor =
                httpContext.RequestServices.GetRequiredService<IScopedInstance<ActionContext>>();
            actionContextAccessor.Value = actionContext;

            var actionBindingContextAccessor =
                httpContext.RequestServices.GetRequiredService<IScopedInstance<ActionBindingContext>>();
            actionBindingContextAccessor.Value = GetActionBindingContext(options, actionContext);
            return httpContext;
        }

        private static ActionBindingContext GetActionBindingContext(MvcOptions options, ActionContext actionContext)
        {
            var valueProviderFactoryContext = new ValueProviderFactoryContext(
                actionContext.HttpContext,
                actionContext.RouteData.Values);

            var valueProvider = CompositeValueProvider.Create(
                options.ValueProviderFactories,
                valueProviderFactoryContext);

            return new ActionBindingContext()
            {
                InputFormatters = options.InputFormatters,
                OutputFormatters = options.OutputFormatters, // Not required for model binding.
                ValidatorProvider = new TestModelValidatorProvider(options.ModelValidatorProviders),
                ModelBinder = new CompositeModelBinder(options.ModelBinders),
                ValueProvider = valueProvider
            };
        }
    }
}