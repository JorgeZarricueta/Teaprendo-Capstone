using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Contracts.Analitica;
using TEAprendo.Infrastructure.Persistence;


namespace TEAprendo.Api.Services;


public class AnaliticaSeguimientoService
{
    private readonly ApplicationDbContext _context;


    private static readonly PatronDefinicion[] Patrones =
    [
        new(
            "Transiciones y cambios",
            [
                "transicion",
                "cambio",
                "rutina",
                "regreso",
                "recreo",
                "inesperado"
            ]),

        new(
            "Estímulos ambientales",
            [
                "ruido",
                "sonido",
                "estimulo",
                "volumen",
                "luz",
                "ambiente"
            ]),

        new(
            "Actividades grupales",
            [
                "grupo",
                "grupal",
                "companeros",
                "equipo"
            ]),

        new(
            "Inicio y secuenciación de tareas",
            [
                "iniciar",
                "inicio",
                "instruccion",
                "pasos",
                "tarea",
                "secuencia"
            ]),

        new(
            "Materiales y texturas",
            [
                "material",
                "materiales",
                "textura",
                "texturas",
                "plastilina",
                "plasticina"
            ])
    ];


    public AnaliticaSeguimientoService(
        ApplicationDbContext context)
    {
        _context =
            context;
    }


    public async Task<IndicadoresSeguimientoDto>
        ObtenerIndicadoresAsync(
            Guid alumnoId)
    {
        var observaciones =
            await _context.BehavioralObservations
                .AsNoTracking()
                .Where(x =>
                    x.StudentId == alumnoId)
                .Select(x => new
                {
                    x.Context,
                    x.Behavior,
                    x.Antecedent,
                    x.Consequence,
                    x.Notes
                })
                .ToListAsync();


        var eventosCrisis =
            await _context.CrisisEvents
                .AsNoTracking()
                .Where(x =>
                    x.StudentId == alumnoId)
                .Select(x => new
                {
                    x.Context,
                    x.EventDescription,
                    x.Trigger,
                    x.ImmediateSupport,
                    x.Outcome,
                    x.Notes
                })
                .ToListAsync();


        var intervenciones =
            await _context.Interventions
                .AsNoTracking()
                .Where(x =>
                    x.BehavioralObservation
                        .StudentId == alumnoId)
                .Select(x => new
                {
                    x.Id,
                    x.Strategy
                })
                .ToListAsync();


        var resultados =
            await _context.InterventionResults
                .AsNoTracking()
                .Where(x =>
                    x.Intervention
                        .BehavioralObservation
                        .StudentId == alumnoId)
                .Select(x => new
                {
                    x.InterventionId,
                    x.Effectiveness
                })
                .ToListAsync();


        var registros =
            new List<string>();


        registros.AddRange(
            observaciones.Select(x =>
                Normalizar(
                    string.Join(
                        " ",
                        x.Context,
                        x.Behavior,
                        x.Antecedent,
                        x.Consequence,
                        x.Notes))));


        registros.AddRange(
            eventosCrisis.Select(x =>
                Normalizar(
                    string.Join(
                        " ",
                        x.Context,
                        x.EventDescription,
                        x.Trigger,
                        x.ImmediateSupport,
                        x.Outcome,
                        x.Notes))));


        var patrones =
            Patrones
                .Select(patron =>
                {
                    var ocurrencias =
                        registros.Count(registro =>
                            patron.Terminos.Any(
                                termino =>
                                    registro.Contains(
                                        termino,
                                        StringComparison.OrdinalIgnoreCase)));


                    return new PatronRecurrenciaDto
                    {
                        Nombre =
                            patron.Nombre,

                        Ocurrencias =
                            ocurrencias,

                        EsRecurrente =
                            ocurrencias >= 2
                    };
                })
                .Where(x =>
                    x.Ocurrencias > 0)
                .OrderByDescending(x =>
                    x.Ocurrencias)
                .ToList();


        var estrategias =
            intervenciones
                .GroupBy(
                    x => x.Strategy.Trim(),
                    StringComparer.OrdinalIgnoreCase)
                .Select(grupo =>
                {
                    var ids =
                        grupo
                            .Select(x => x.Id)
                            .ToHashSet();


                    var resultadosEstrategia =
                        resultados
                            .Where(x =>
                                ids.Contains(
                                    x.InterventionId))
                            .ToList();


                    var alta =
                        resultadosEstrategia.Count(x =>
                            EsEfectividad(
                                x.Effectiveness,
                                "Alta"));


                    var media =
                        resultadosEstrategia.Count(x =>
                            EsEfectividad(
                                x.Effectiveness,
                                "Media"));


                    var baja =
                        resultadosEstrategia.Count(x =>
                            EsEfectividad(
                                x.Effectiveness,
                                "Baja"));


                    var evaluados =
                        alta +
                        media +
                        baja;


                    var promedio =
                        evaluados == 0
                            ? 0
                            : (
                                (alta * 3.0) +
                                (media * 2.0) +
                                baja
                              ) / evaluados;


                    return new EstrategiaEfectividadDto
                    {
                        Estrategia =
                            grupo.Key,

                        Aplicaciones =
                            grupo.Count(),

                        ResultadosRegistrados =
                            resultadosEstrategia.Count,

                        Alta =
                            alta,

                        Media =
                            media,

                        Baja =
                            baja,

                        PuntajePromedio =
                            Math.Round(
                                promedio,
                                2),

                        Clasificacion =
                            ClasificarEfectividad(
                                promedio,
                                evaluados),

                        EvaluacionSuficiente =
                            grupo.Count() >= 2 &&
                            evaluados >= 2
                    };
                })
                .OrderByDescending(x =>
                    x.EvaluacionSuficiente)
                .ThenByDescending(x =>
                    x.PuntajePromedio)
                .ToList();


        return new IndicadoresSeguimientoDto
        {
            AlumnoId =
                alumnoId,

            TotalObservaciones =
                observaciones.Count,

            TotalEventosCrisis =
                eventosCrisis.Count,

            TotalIntervenciones =
                intervenciones.Count,

            TotalResultados =
                resultados.Count,

            ResultadosAlta =
                resultados.Count(x =>
                    EsEfectividad(
                        x.Effectiveness,
                        "Alta")),

            ResultadosMedia =
                resultados.Count(x =>
                    EsEfectividad(
                        x.Effectiveness,
                        "Media")),

            ResultadosBaja =
                resultados.Count(x =>
                    EsEfectividad(
                        x.Effectiveness,
                        "Baja")),

            SuficienciaContexto =
                observaciones.Count +
                eventosCrisis.Count >= 3,

            SuficienciaEfectividad =
                resultados.Count >= 3,

            Patrones =
                patrones,

            Estrategias =
                estrategias
        };
    }


    private static bool EsEfectividad(
        string valor,
        string esperada)
    {
        return string.Equals(
            valor.Trim(),
            esperada,
            StringComparison.OrdinalIgnoreCase);
    }


    private static string ClasificarEfectividad(
        double promedio,
        int cantidad)
    {
        if (cantidad == 0)
        {
            return "Sin resultados";
        }


        if (promedio >= 2.5)
        {
            return "Alta";
        }


        if (promedio >= 1.5)
        {
            return "Media";
        }


        return "Baja";
    }


    private static string Normalizar(
        string valor)
    {
        var texto =
            valor
                .ToLowerInvariant()
                .Normalize(
                    NormalizationForm.FormD);


        var resultado =
            new StringBuilder();


        foreach (var caracter in texto)
        {
            if (CharUnicodeInfo
                .GetUnicodeCategory(
                    caracter) ==
                UnicodeCategory.NonSpacingMark)
            {
                continue;
            }


            resultado.Append(
                char.IsLetterOrDigit(
                    caracter)
                    ? caracter
                    : ' ');
        }


        return resultado
            .ToString()
            .Normalize(
                NormalizationForm.FormC);
    }


    private sealed record PatronDefinicion(
        string Nombre,
        string[] Terminos);
}