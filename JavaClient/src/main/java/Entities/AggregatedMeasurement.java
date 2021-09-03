package Entities;

import java.sql.Date;
import java.text.DateFormat;

public class AggregatedMeasurement {
    double value;
    Date intervalStartTime;

    @Override
    public String toString() {
        return "Current at " + DateFormat.getInstance().format(intervalStartTime) + ": " + value +" A";
    }
}
