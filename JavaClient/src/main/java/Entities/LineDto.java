package Entities;

import java.util.List;
import java.util.UUID;

public class LineDto {
    public UUID id;
    public String name;
    public String owner;
    public List<SpanDto> spans;
}
