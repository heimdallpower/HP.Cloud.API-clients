// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import Clients.AccessTokenClient;

import java.io.IOException;
import java.util.Properties;
import Services.HeimdallApiService;


class CallHeimdallApi {

    private static String authority;
    private static String clientId;
    private static String clientSecret;
    private static String backendClientId;
    private static String policy;
    private static String scope;
    private static String instance;
    private static String domain;
    private static String apiUrl;

    public static void main(String args[]) throws Exception{

        loadProperties();

        try {
            System.out.println("\nHello Heimdall!\n");

            AccessTokenClient client = new AccessTokenClient(clientId, clientSecret, authority, scope);
            String accessToken = client.getAccessToken();
            System.out.println("\nAccess token = \n" + accessToken);

            HeimdallApiService service = new HeimdallApiService(accessToken, apiUrl);
            service.Run();

            System.out.println("Press any key to exit ...");
            System.in.read();

        } catch(Exception ex){
            System.out.println("Oops! We have an exception of type - " + ex.getClass());
            System.out.println("Exception message - " + ex.getMessage());
            throw ex;
        }
    }

    /**
     * Helper function unique to this sample setting. In a real application these wouldn't be so hardcoded, for example
     * different users may need different authority endpoints and the key/cert paths could come from a secure keyvault
     */
    private static void loadProperties() throws IOException {
        // Load properties file and set properties used throughout the sample
        Properties properties = new Properties();
        properties.load(Thread.currentThread().getContextClassLoader().getResourceAsStream("application.properties"));
        clientId = properties.getProperty("CLIENT_ID");
        clientSecret = properties.getProperty("CLIENT_SECRET");
        policy = properties.getProperty("POLICY");

        Boolean useDeveloperApi = Boolean.parseBoolean(properties.getProperty("USE_DEVELOPER_API"));

        instance = useDeveloperApi ? 
            "https://hpadb2cdev.b2clogin.com" : 
            "https://hpadb2cprod.b2clogin.com";
        domain = useDeveloperApi ?
            "hpadb2cdev.onmicrosoft.com" : 
            "hpadb2cprod.onmicrosoft.com";
        backendClientId = useDeveloperApi ? 
            "f2fd8894-ae2e-4965-8318-e6c6781b5b80" : 
            "dc5758ae-4eea-416e-9e61-812914d9a49a";
        scope = String.format("https://%s/%s/.default", domain, backendClientId);
        authority = String.format("%s/tfp/%s/%s", instance, domain, policy);
        apiUrl = useDeveloperApi ? 
            "https://api.heimdallcloud-dev.com/" : 
            "https://api.heimdallcloud.com/";
    }   
}



