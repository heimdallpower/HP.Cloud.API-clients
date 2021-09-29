package Entities;

import java.util.List;
import java.util.UUID;

public class SpanDto {
    public UUID id;
    public List<SpanPhaseDto> spanPhases;
    
    public String toString() {
        return "\nSpan " + id.toString() + " with phases = " + spanPhases;
    }
}
