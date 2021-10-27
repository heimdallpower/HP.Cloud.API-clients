// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import Clients.AccessTokenClient;

import java.io.IOException;
import java.util.Properties;
import Services.HeimdallApiService;


class CallHeimdallApi {

    private static String authority;
    private static String clientId;
    private static String scope;
    private static String keyPath;
    private static String certPath;
    private static String apiUrl;

    public static void main(String args[]) throws Exception{

        loadProperties();

        try {
            System.out.println("Hello Heimdall!");

            AccessTokenClient client = new AccessTokenClient(keyPath, certPath, clientId, authority, scope);
            String accessToken = client.getAccessToken();
            System.out.println("Access token = " + accessToken);

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
        authority = "https://login.microsoftonline.com/132d3d43-145b-4d30-aaf3-0a47aa7be073/";
        clientId = properties.getProperty("CLIENT_ID");
        keyPath = properties.getProperty("PKCS8_PRIVATE_KEY_PATH");
        certPath = properties.getProperty("CRT_CERTIFICATE_PATH");

        Boolean useDeveloperApi = Boolean.parseBoolean(properties.getProperty("USE_DEVELOPER_API"));

        scope = useDeveloperApi ? 
            "6b9ba5c0-4a21-4263-bbf5-8c4e30c0ee1b/.default": 
            "aac6dec0-4c1b-4565-a825-5bb9401a1547/.default";
        apiUrl = useDeveloperApi ? 
            "https://api.heimdallcloud-dev.com/": 
            "https://api.heimdallcloud.com/";
    }   
}



