﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.JsonPatch.Test
{
    public class TestErrorLogger<T> where T: class
    {
        public string ErrorMessage { get; set; }

        public void LogErrorMessage(JsonPatchError<T> patchError)
        {
            ErrorMessage = patchError.ErrorMessage;
        }
    }
}
