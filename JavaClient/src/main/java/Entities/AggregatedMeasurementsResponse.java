package Entities;

import java.util.List;

public class AggregatedMeasurementsResponse {
    public List<AggregatedMeasurement> data;
    public String message;

    public String toString() {
        return "AggregatedMeasurementResponse (" + 
            "message= " + message +
            ", Measurement count= " + data.size();
    }
}
