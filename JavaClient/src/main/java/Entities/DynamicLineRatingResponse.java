package Entities;

import java.util.List;

public class DynamicLineRatingResponse {
    public List<DynamicLineRating> data;
    public String message;

    public String toString() {
        return "DynamicLineRatingResponse (" + 
            "message= " + message +
            ", Measurement count= " + data.size();
    }
}
