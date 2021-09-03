package Entities;

import java.sql.Date;
import java.text.DateFormat;

public class DynamicLineRating {
    double ampacity;
    Date intervalStartTime;

    @Override
    public String toString() {
        return "Dynamic line rating at " + DateFormat.getInstance().format(intervalStartTime) + ": " + ampacity +" A";
    }
}
