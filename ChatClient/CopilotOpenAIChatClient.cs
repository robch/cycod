using System;
using OpenAI.Chat;
using OpenAI;
using System.ClientModel.Primitives;

public class CopilotOpenAIChatClient : ChatClient
{
    public CopilotOpenAIChatClient(string model, string hmacKey, string integrationId, string endpoint)
        : base(model: model, credential: null, options: GetOptionsWithPolicies(hmacKey, integrationId))
    {
        _hmacKey = hmacKey;
        _integrationId = integrationId;
        _endpoint = endpoint;
    }

    private static OpenAIClientOptions GetOptionsWithPolicies(string hmacKey, string integrationId)
    {
        OpenAIClientOptions options = new OpenAIClientOptions();

        // Compute the HMAC header value using our HMAC helper
        string hmacValue = HMACHelper.Encode(hmacKey);
        
        // Add the custom header for integration ID
        options.AddPolicy(new CustomHeaderPolicy("Copilot-Integration-Id", integrationId), PipelinePosition.BeforeTransport);
        
        // Add the custom header for HMAC authentication
        options.AddPolicy(new CustomHeaderPolicy("Request-HMAC", hmacValue), PipelinePosition.BeforeTransport);

        return options;
    }

    private readonly string _hmacKey;
    private readonly string _integrationId;
    private readonly string _endpoint;
}
