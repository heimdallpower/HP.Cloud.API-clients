package Entities;

import java.util.List;

public class LineDto {
    public String name;
    public String owner;
    public List<LineSpanDto> lineSpans;
    
    public String toString() {
        return "\nName: " + name +
            "\nOwner: " + owner +
            "\nLinespans in line: " + lineSpans.size() + 
            "\n" + lineSpans;
    }
}
