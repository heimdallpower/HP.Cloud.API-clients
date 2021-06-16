// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonElement;
import com.google.gson.JsonParser;
import com.microsoft.aad.msal4j.ClientCredentialFactory;
import com.microsoft.aad.msal4j.ClientCredentialParameters;
import com.microsoft.aad.msal4j.ConfidentialClientApplication;
import com.microsoft.aad.msal4j.IAuthenticationResult;
import com.nimbusds.oauth2.sdk.http.HTTPResponse;
import java.io.BufferedReader;
import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.security.KeyFactory;
import java.security.PrivateKey;
import java.security.cert.CertificateFactory;
import java.security.cert.X509Certificate;
import java.security.spec.PKCS8EncodedKeySpec;
import java.sql.Date;
import java.text.DateFormat;
import java.util.Collections;
import java.util.List;
import java.util.Properties;
import java.util.concurrent.CompletableFuture;

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
            IAuthenticationResult result = getAccessTokenByClientCredentialGrant();
            System.out.println("Access token = " + result.accessToken());
            List<Measurement> measurements = getMeasurementsForPowerLine(result.accessToken());
            for(Measurement measurement : measurements) {
                System.out.println(measurement.toString());
            }

            System.out.println("Measurements found: " + measurements.size());
            System.out.println("Press any key to exit ...");
            System.in.read();

        } catch(Exception ex){
            System.out.println("Oops! We have an exception of type - " + ex.getClass());
            System.out.println("Exception message - " + ex.getMessage());
            throw ex;
        }
    }

    private static IAuthenticationResult getAccessTokenByClientCredentialGrant() throws Exception {

        PKCS8EncodedKeySpec spec = new PKCS8EncodedKeySpec(Files.readAllBytes(Paths.get(keyPath)));
        PrivateKey key = KeyFactory.getInstance("RSA").generatePrivate(spec);

        InputStream certStream = new ByteArrayInputStream(Files.readAllBytes(Paths.get(certPath)));
        X509Certificate cert = (X509Certificate) CertificateFactory.getInstance("X.509").generateCertificate(certStream);

        ConfidentialClientApplication app = ConfidentialClientApplication.builder(
                clientId,
                ClientCredentialFactory.createFromCertificate(key, cert))
                .authority(authority)
                .build();

        // With client credentials flows the scope is ALWAYS of the shape "resource/.default", as the
        // application permissions need to be set statically (in the portal), and then granted by a tenant administrator

        ClientCredentialParameters clientCredentialParam = ClientCredentialParameters.builder(
                Collections.singleton(scope))
                .build();

        CompletableFuture<IAuthenticationResult> future = app.acquireToken(clientCredentialParam);
        return future.get();
    }

    private static List<Measurement> getMeasurementsForPowerLine(String accessToken) throws IOException {

        int neuronId = 703;
        Date toDate = new Date(System.currentTimeMillis());
        Date fromDate = new Date((long) (System.currentTimeMillis() - (7 * 8.64e+7))); // 8.64e+7 = 1 day in milliseconds

        StringBuilder urlBuilder = new StringBuilder();
        urlBuilder
            .append(apiUrl + "api/beta/measurements")
            // You can also use the more detailed format: yyyy-MM-dd'T'HH:mm:ss'Z'
            .append("?fromDateTime=" + fromDate.toString())
            .append("&toDateTime=" + toDate.toString())
            .append("&neuronId=" + neuronId);

        System.out.println("Sending request to url: " + urlBuilder.toString());
    
        URL url = new URL(urlBuilder.toString());

        HttpURLConnection conn = (HttpURLConnection) url.openConnection();
        conn.setRequestMethod("GET");
        conn.setRequestProperty("Authorization", "Bearer " + accessToken);

        int httpResponseCode = conn.getResponseCode();

        if(httpResponseCode == HTTPResponse.SC_OK) {

            StringBuilder response;
            try(BufferedReader in = new BufferedReader(
                    new InputStreamReader(conn.getInputStream()))){

                String inputLine;
                response = new StringBuilder();
                while (( inputLine = in.readLine()) != null) {
                    response.append(inputLine);
                }
            }
            Gson gson = new GsonBuilder().setDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'").create();
            MeasurementResponse measurementResponse = gson.fromJson(response.toString(),MeasurementResponse.class);

            return measurementResponse.data;
        } else {
            StringBuilder errorResponse;
            try(BufferedReader in = new BufferedReader(
                    new InputStreamReader(conn.getErrorStream()))){

                String inputLine;
                errorResponse = new StringBuilder();
                while (( inputLine = in.readLine()) != null) {
                    errorResponse.append(inputLine);
                }
            }
        
            System.out.println(
                String.format("Connection returned HTTP code: %s with message: %s, details:\n%s",
                    httpResponseCode, 
                    conn.getResponseMessage(),
                    prettifyJsonString(errorResponse.toString())
                ));


            return Collections.emptyList();
        }
    }

    private static String prettifyJsonString(String json){
        JsonParser parser = new JsonParser();
        Gson gson = new GsonBuilder().disableHtmlEscaping().setPrettyPrinting().create();

        JsonElement el = parser.parse(json);
        return gson.toJson(el);
    }

    /**
     * Helper function unique to this sample setting. In a real application these wouldn't be so hardcoded, for example
     * different users may need different authority endpoints and the key/cert paths could come from a secure keyvault
     */
    private static void loadProperties() throws IOException {
        // Load properties file and set properties used throughout the sample
        Properties properties = new Properties();
        properties.load(Thread.currentThread().getContextClassLoader().getResourceAsStream("application.properties"));
        authority = properties.getProperty("AUTHORITY");
        clientId = properties.getProperty("CLIENT_ID");
        keyPath = properties.getProperty("PKCS8_PRIVATE_KEY_PATH");
        certPath = properties.getProperty("CRT_CERTIFICATE_PATH");
        scope = properties.getProperty("SCOPE");
        apiUrl = properties.getProperty("API_URL");
    }

    public static  class MeasurementResponse {
        List<Measurement> data;
        int code;
        String message;
        public String toString() {
            return "HeimdallResponse (" + 
                "data= " + data +
                ", code= " + code +
                ", message= " + message +
            ")";
        }
    }
    public static class Measurement {
        double current;
        double wireTemperature;
        double housingTemperature;
        Date timeObserved;
        int neuronId;

        @Override
        public String toString() {
            return "Measurement (" + 
                "current= " + current +
                ", wireTemperature= " + wireTemperature +
                ", housingTemperature= " + housingTemperature +
                ", timeObserved= " + DateFormat.getInstance().format(timeObserved) +
                ", neuronId= " + neuronId +
            ")";
        }
    }
}


