namespace TEAprendo.Contracts.Analitica;


public class IndicadoresSeguimientoDto
{
    public Guid AlumnoId { get; set; }

    public int TotalObservaciones { get; set; }

    public int TotalEventosCrisis { get; set; }

    public int TotalIntervenciones { get; set; }

    public int TotalResultados { get; set; }

    public int ResultadosAlta { get; set; }

    public int ResultadosMedia { get; set; }

    public int ResultadosBaja { get; set; }

    public bool SuficienciaContexto { get; set; }

    public bool SuficienciaEfectividad { get; set; }

    public List<PatronRecurrenciaDto> Patrones { get; set; }
        = [];

    public List<EstrategiaEfectividadDto> Estrategias { get; set; }
        = [];
}


public class PatronRecurrenciaDto
{
    public string Nombre { get; set; }
        = string.Empty;

    public int Ocurrencias { get; set; }

    public bool EsRecurrente { get; set; }
}


public class EstrategiaEfectividadDto
{
    public string Estrategia { get; set; }
        = string.Empty;

    public int Aplicaciones { get; set; }

    public int ResultadosRegistrados { get; set; }

    public int Alta { get; set; }

    public int Media { get; set; }

    public int Baja { get; set; }

    public double PuntajePromedio { get; set; }

    public string Clasificacion { get; set; }
        = string.Empty;

    public bool EvaluacionSuficiente { get; set; }
}