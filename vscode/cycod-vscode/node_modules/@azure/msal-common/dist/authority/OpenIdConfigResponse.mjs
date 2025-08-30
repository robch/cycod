/*! @azure/msal-common v15.12.0 2025-08-19 */
'use strict';
/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */
function isOpenIdConfigResponse(response) {
    return (response.hasOwnProperty("authorization_endpoint") &&
        response.hasOwnProperty("token_endpoint") &&
        response.hasOwnProperty("issuer") &&
        response.hasOwnProperty("jwks_uri"));
}

export { isOpenIdConfigResponse };
//# sourceMappingURL=OpenIdConfigResponse.mjs.map
