package Services;

import java.io.BufferedReader;
import java.io.IOException;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.URLEncoder;
import java.sql.Date;
import java.util.List;
import java.io.InputStreamReader;
import java.util.stream.Collectors;
import java.util.Collections;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonElement;
import com.google.gson.JsonParser;
import com.nimbusds.oauth2.sdk.http.HTTPResponse;

import Entities.*;
import Enums.*;

public class HeimdallApiService {
    private String accessToken;
    private String apiUrl;

    public HeimdallApiService(String accessToken, String apiUrl){
        this.accessToken = accessToken;
        this.apiUrl = apiUrl;
    }
    
    public void Run() throws IOException{
        List<LineDto> lines = getLines();

        for(LineDto line : lines){
            System.out.println("\nRequesting data for line with name: " + line.name);

            List<AggregatedMeasurement> measurements = getAggregatedMeasurementsForLine(accessToken, line);
            for(AggregatedMeasurement measurement : measurements) {
                System.out.println(measurement.toString());
            }
            System.out.println("Measurements found in the last week: " + measurements.size());                
            
            List<DynamicLineRating> dynamicLineRatings = getDynamicLineRatingsForLine(accessToken, line, DLRType.HP);
            for(DynamicLineRating dynamicLineRating : dynamicLineRatings) {
                System.out.println(dynamicLineRating.toString());
            }
            System.out.println("Dynamic line ratings found in the last week: " + measurements.size());
        }
    }

    private List<LineDto> getLines() throws IOException {
        StringBuilder urlBuilder = new StringBuilder();
        urlBuilder.append(apiUrl + "api/v1/lines");
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
            LineResponse lineResponse = gson.fromJson(response.toString(),LineResponse.class);
            
            ;
            System.out.println(prettifyJsonString(response.toString()));
            System.out.println("\nRequest data with the ids of lines, spans, and span phases");

            List<LineDto> lines = lineResponse.data;
            return lines;
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
            System.out.println("Response: " + conn.getResponseMessage());
        
            System.out.println(
                String.format("Connection returned HTTP code: %s with message: %s, details:\n%s",
                    httpResponseCode, 
                    conn.getResponseMessage(),
                    prettifyJsonString(errorResponse.toString())
                ));


            return Collections.emptyList();
        }
    }

    private List<AggregatedMeasurement> getAggregatedMeasurementsForLine(String accessToken, LineDto line) throws IOException {

        Date toDate = new Date(System.currentTimeMillis());
        Date fromDate = new Date((long) (System.currentTimeMillis() - (7 * 8.64e+7))); // 8.64e+7 = 1 day in milliseconds

        String endpointUrl = apiUrl + "api/v1/aggregated-measurements";
        StringBuilder paramsBuilder = new StringBuilder();
        paramsBuilder
            // You can also use the more detailed format: yyyy-MM-dd'T'HH:mm:ss'Z'
            .append("?fromDateTime=" + fromDate.toString())
            .append("&toDateTime=" + toDate.toString())
            .append("&measurementType=" + MeasurementType.Current)
            .append("&aggregationType=" + AggregationType.Max)
            .append("&intervalDuration=" + IntervalDuration.EveryDay)
            .append("&lineId=" + line.id.toString());


        String encodedUrl  = endpointUrl + paramsBuilder.toString();
        System.out.println("Sending measurement request to url: " + encodedUrl);
    
        URL url = new URL(encodedUrl);

        HttpURLConnection conn = (HttpURLConnection) url.openConnection();
        conn.setRequestMethod("GET");
        conn.setRequestProperty("Authorization", "Bearer " + accessToken);

        int httpResponseCode = conn.getResponseCode();

        if(httpResponseCode == HTTPResponse.SC_OK) {

            StringBuilder responseBuilder;
            try(BufferedReader in = new BufferedReader(
                    new InputStreamReader(conn.getInputStream()))){

                String inputLine;
                responseBuilder = new StringBuilder();
                while (( inputLine = in.readLine()) != null) {
                    responseBuilder.append(inputLine);
                }
            }
            Gson gson = new GsonBuilder().setDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'").create();
            AggregatedMeasurementsResponse response = gson.fromJson(responseBuilder.toString(),AggregatedMeasurementsResponse.class);

            return response.data;
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
                "Measurement request failed: " + conn.getResponseMessage() + errorResponse);
        
            System.out.println(
                String.format("Connection returned HTTP code: %s with message: %s, details:\n%s",
                    httpResponseCode, 
                    conn.getResponseMessage(),
                    prettifyJsonString(errorResponse.toString())
                ));


            return Collections.emptyList();
        }
    }      
    
    private List<DynamicLineRating> getDynamicLineRatingsForLine(String accessToken, LineDto line, DLRType dlrType) throws IOException {

        Date toDate = new Date(System.currentTimeMillis());
        Date fromDate = new Date((long) (System.currentTimeMillis() - (7 * 8.64e+7))); // 8.64e+7 = 1 day in milliseconds

        String endpointUrl = apiUrl + "api/beta/dlr/aggregated-dlr";
        StringBuilder paramsBuilder = new StringBuilder();
        paramsBuilder
            // You can also use the more detailed format: yyyy-MM-dd'T'HH:mm:ss'Z'
            .append("?fromDateTime=" + fromDate.toString())
            .append("&toDateTime=" + toDate.toString())
            .append("&dlrType=" + dlrType)
            .append("&intervalDuration=" + IntervalDuration.EveryDay)
            .append("&lineName=" + URLEncoder.encode(line.name, "UTF-8"));


        String encodedUrl  = endpointUrl + paramsBuilder.toString();
        System.out.println("Sending DLR request to url: " + encodedUrl);
    
        URL url = new URL(encodedUrl);

        HttpURLConnection conn = (HttpURLConnection) url.openConnection();
        conn.setRequestMethod("GET");
        conn.setRequestProperty("Authorization", "Bearer " + accessToken);

        int httpResponseCode = conn.getResponseCode();

        if(httpResponseCode == HTTPResponse.SC_OK) {

            StringBuilder responseBuilder;
            try(BufferedReader in = new BufferedReader(
                    new InputStreamReader(conn.getInputStream()))){

                String inputLine;
                responseBuilder = new StringBuilder();
                while (( inputLine = in.readLine()) != null) {
                    responseBuilder.append(inputLine);
                }
            }
            Gson gson = new GsonBuilder().setDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'").create();
            DynamicLineRatingResponse response = gson.fromJson(responseBuilder.toString(),DynamicLineRatingResponse.class);

            return response.data;
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
                "DLR request failed: " + conn.getResponseMessage() + errorResponse);
        
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
        Gson gson = new GsonBuilder().setPrettyPrinting().create();

        JsonElement el = parser.parse(json);
        return gson.toJson(el);
    }

}
