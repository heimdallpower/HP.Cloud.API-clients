package Entities;

import java.util.List;

public class LineResponse {
    public List<LineDto> data;
    public int code;
    public String message;
    
    public String toString() {
        return "LineResponse (" + 
            "data= " + data +
            ", code= " + code +
            ", message= " + message +
        ")";
    }
}
