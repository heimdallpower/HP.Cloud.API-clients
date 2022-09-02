from enum import Enum   

class MeasurementType(str, Enum):
    Current = 'Current'
    WireTemperature = 'WireTemperature'

class IntervalDuration(str, Enum):
    Every5Minutes = 'PT5M'
    EveryDay = 'P1D'

class AggregationType(str, Enum):
    MIN = 'Min'
    MAX = 'Max'
    AVERAGE = 'Average'

class DLRType(str, Enum):
    HP = 'HP'
    Cigre = 'Cigre'
    IEEE = 'IEEE'