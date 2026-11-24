using System;
using System.Collections.Generic;
using UnityEngine;

namespace HastaEncontrarles.Nivel4
{
    [Serializable]
    public class DatosMigrante
    {
        public int id;
        public string nombreCompleto;
        public string expediente;
        public string fechaDesaparicion;
        public string oficioEdad;
        public string historiaMemoria;
        public string mensajeBordado;
        public string nombreRecursoFoto;
        public bool panueloColocado;
    }

    [Serializable]
    public class LineaDialogo
    {
        public string hablante;
        public string cargo;
        [TextArea(2, 5)]
        public string texto;
        public string notaEducativa;
    }

    [Serializable]
    public class PreguntaAutodidacta
    {
        public string tituloFase;
        public string pregunta;
        public string[] opciones;
        public int indiceCorrecto;
        public string explicacionCorrecta;
        public string explicacionIncorrecta;
    }

    public static class DatosMemoriaNivel4
    {
        public const string TITULO_JUEGO = "HASTA ENCONTRARLES: EL CAMINO DE LA ESPERANZA";
        public const string SUBTITULO_NIVEL4 = "Nivel 4: La Memoria Viva y el Reclamo de Justicia";
        public const string EQUIPO_AUTORES = "Valeria Guadalupe Calvillo Mendoza y María del Carmen Vargas Martínez";
        public const string INSTITUCION = "Universidad Tecnológica del Norte de Guanajuato (UTNG) - Grupo GIDS06001";
        public const string DOCENTE = "Docente: Gabriel Barrón";

        public static List<DatosMigrante> ObtenerLos22Ausentes()
        {
            return new List<DatosMigrante>
            {
                new DatosMigrante {
                    id = 1,
                    nombreCompleto = "Fernando Guzmán Ramírez",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Joven trabajador de San Luis de la Paz",
                    historiaMemoria = "Partió aquel 21 de marzo con la ilusión de apoyar a su familia en San Luis de la Paz. Su juventud y sonrisa permanecen vivas en cada jornada de búsqueda de su madre y del colectivo.",
                    mensajeBordado = "«Por tu sonrisa que guía nuestros pasos, no descansaremos hasta abrazarte.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_01"
                },
                new DatosMigrante {
                    id = 2,
                    nombreCompleto = "José Antonio Guerrero López",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Padre de familia y sustento del hogar",
                    historiaMemoria = "Hombre trabajador y solidario con sus vecinos. Salió buscando un futuro digno ante la falta de empleo en la región.",
                    mensajeBordado = "«Tu nombre está bordado con hilo verde de esperanza en el corazón del pueblo.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_02"
                },
                new DatosMigrante {
                    id = 3,
                    nombreCompleto = "Miguel Ángel Ramírez Araiza",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Joven ludovicense lleno de sueños",
                    historiaMemoria = "Conocido por su nobleza y dedicación a su hogar. Su familia transformó la espera en organización comunitaria para exigir verdad.",
                    mensajeBordado = "«Ningún silencio borrará tus pasos; te buscamos con amor incondicional.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_03"
                },
                new DatosMigrante {
                    id = 4,
                    nombreCompleto = "José Luis Duarte Cruz",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Hermano y trabajador del campo",
                    historiaMemoria = "Emprendió el viaje junto a su hermano Juan Manuel y compañeros de comunidad. Su familia mantiene encendida la luz de su regreso.",
                    mensajeBordado = "«En cada flor de cempasúchil y en cada puntada vive nuestra promesa.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_04"
                },
                new DatosMigrante {
                    id = 5,
                    nombreCompleto = "Rafael Paz Guerrero",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Joven migrante de San Luis de la Paz",
                    historiaMemoria = "Sus seres queridos recuerdan su alegría y su deseo de salir adelante honradamente. El colectivo reclama justicia por su camino.",
                    mensajeBordado = "«La memoria es nuestra resistencia contra el olvido.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_05"
                },
                new DatosMigrante {
                    id = 6,
                    nombreCompleto = "Juan Manuel Rojas Pérez",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Oficio de construcción y campo",
                    historiaMemoria = "Hombre de palabra y esfuerzo diario. Su silla en casa lo espera mientras las madres buscadoras recorren caminos y oficinas.",
                    mensajeBordado = "«Buscarles es un acto de amor que vence al miedo.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_06"
                },
                new DatosMigrante {
                    id = 7,
                    nombreCompleto = "Santos Eloy Peralta Hernández",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Trabajador comunitario",
                    historiaMemoria = "Siempre dispuesto a tender la mano en su colonia. Su familia forma parte del corazón del Colectivo Justicia y Esperanza.",
                    mensajeBordado = "«Tu dignidad camina con nosotras en cada brigada.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_07"
                },
                new DatosMigrante {
                    id = 8,
                    nombreCompleto = "Antonio Coronilla Luna",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Campesino y hombre de familia",
                    historiaMemoria = "De raíces profundas en la tierra guanajuatense. Salió junto a Gregorio Coronilla buscando bienestar para los suyos.",
                    mensajeBordado = "«El viento de la sierra guarda tu voz; San Luis de la Paz te nombra hoy.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_08"
                },
                new DatosMigrante {
                    id = 9,
                    nombreCompleto = "José Humberto Morín López",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Joven estudiante y obrero",
                    historiaMemoria = "Sus ojos firmes reflejan la esperanza de toda una generación que tuvo que migrar por falta de oportunidades locales.",
                    mensajeBordado = "«Bordo tu nombre para que la historia jamás mire hacia otro lado.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_09"
                },
                new DatosMigrante {
                    id = 10,
                    nombreCompleto = "Alejandro Castillo Ramírez",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Trabajador agrícola",
                    historiaMemoria = "Amante de su tierra y de su familia. Su fotografía en el kiosco central nos recuerda que cada vida cuenta.",
                    mensajeBordado = "«Cada hilo verde une nuestro corazón con tu recuerdo.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_10"
                },
                new DatosMigrante {
                    id = 11,
                    nombreCompleto = "Raúl Trejo Medina",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Joven trabajador de San Luis de la Paz",
                    historiaMemoria = "Recordado por su fortaleza y cariño hacia sus padres. Su familia ha sostenido la exigencia ante instancias nacionales e internacionales.",
                    mensajeBordado = "«La verdad nos hará libres; la memoria nos mantiene unidos.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_11"
                },
                new DatosMigrante {
                    id = 12,
                    nombreCompleto = "Valentín Alamilla Camacho",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Albañil y padre dedicado",
                    historiaMemoria = "Acostumbrado al trabajo duro bajo el sol. Hoy las buscadoras caminan bajo ese mismo sol con la varilla en alto por él y sus compañeros.",
                    mensajeBordado = "«Ni la tierra ni el tiempo ocultarán la justicia que mereces.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_12"
                },
                new DatosMigrante {
                    id = 13,
                    nombreCompleto = "Ricardo Salazar Sánchez",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Joven profesionista y trabajador",
                    historiaMemoria = "Con metas claras y profundo respeto por su familia. Su caso simboliza la urgencia de protocolos forenses dignos y científicos.",
                    mensajeBordado = "«Con ciencia, amor y memoria reconstruimos el camino hacia ti.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_13"
                },
                new DatosMigrante {
                    id = 14,
                    nombreCompleto = "Juan Manuel Duarte Cruz",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Joven campesino ludovicense",
                    historiaMemoria = "Hermano de José Luis Duarte Cruz. La doble ausencia en su hogar multiplicó la fuerza de su madre y hermanas para fundar el colectivo.",
                    mensajeBordado = "«Dos hermanos, una misma esperanza que jamás se apaga.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_14"
                },
                new DatosMigrante {
                    id = 15,
                    nombreCompleto = "Héctor Castillo Salazar",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Trabajador y amigo leal",
                    historiaMemoria = "Su risa franca y su compañerismo siguen presentes en las reuniones de las familias de San Luis de la Paz.",
                    mensajeBordado = "«Aquí nadie se rinde; tu comunidad te abraza en este mural.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_15"
                },
                new DatosMigrante {
                    id = 16,
                    nombreCompleto = "Gregorio Coronilla Luna",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Hombre de campo y tradición",
                    historiaMemoria = "Orgulloso de sus raíces en Guanajuato. Partió con la promesa de volver pronto tras la temporada de trabajo.",
                    mensajeBordado = "«Tus raíces están en esta plaza donde hoy florece tu memoria.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_16"
                },
                new DatosMigrante {
                    id = 17,
                    nombreCompleto = "Ángel Padrón Sandoval",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Agricultor y jefe de familia",
                    historiaMemoria = "Hombre sereno y protector. Su familia ha recorrido hospitales, semefos y caravanas manteniendo vivo su legado.",
                    mensajeBordado = "«Por cada lágrima sembrada, cosecharemos verdad y justicia.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_17"
                },
                new DatosMigrante {
                    id = 18,
                    nombreCompleto = "José Manuel Pérez Guerrero",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Joven migrante de San Luis de la Paz",
                    historiaMemoria = "Uno de los más jóvenes del grupo. Su madre y tías han enseñado a toda la comunidad que un hijo se busca siempre.",
                    mensajeBordado = "«Eres luz en nuestras manos y fuerza en nuestra voz.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_18"
                },
                new DatosMigrante {
                    id = 19,
                    nombreCompleto = "Isidro González Coronilla",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Trabajador rural",
                    historiaMemoria = "Desapareció el 21 de marzo de 2011 rumbo al norte. Su sombrero y su mirada honesta son emblema de dignidad campesina.",
                    mensajeBordado = "«La tierra que trabajaste hoy exige que regreses a casa.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_19"
                },
                new DatosMigrante {
                    id = 20,
                    nombreCompleto = "Miguel Jaramillo Palacios",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Músico y trabajador comunitario",
                    historiaMemoria = "Siempre presente en las celebraciones de su familia. Hoy las notas de guitarra acústica en la plaza suenan en su honor.",
                    mensajeBordado = "«Como un acorde que no termina, tu recuerdo resuena en libertad.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_20"
                },
                new DatosMigrante {
                    id = 21,
                    nombreCompleto = "Samuel Guzmán Castañeda",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Joven emprendedor y deportista",
                    historiaMemoria = "Su energía y sueños quedaron grabados en su barrio. El Colectivo Justicia y Esperanza lleva su rostro en cada marcha.",
                    mensajeBordado = "«Caminamos juntas para que ningún joven vuelva a desaparecer.»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_21"
                },
                new DatosMigrante {
                    id = 22,
                    nombreCompleto = "Mariano Luna Jiménez",
                    expediente = "AP/PGR/SIEDO/UEITMIO/067/2012",
                    fechaDesaparicion = "21 de marzo de 2011",
                    oficioEdad = "Trabajador y pilar de su familia",
                    historiaMemoria = "Confirmado e identificado gracias a la labor independiente del Equipo Argentino de Antropología Forense (EAAF) en San Fernando, Tamaulipas. Su restitución digna impulsa a seguir buscando a los otros 21.",
                    mensajeBordado = "«La ciencia forense y el amor de tu familia te devolvieron tu nombre. ¡Hasta encontrarles a todos!»",
                    nombreRecursoFoto = "Sprites/Migrantes/migrante_22"
                }
            };
        }

        public static List<LineaDialogo> ObtenerDialogoPeritoEAAF()
        {
            return new List<LineaDialogo>
            {
                new LineaDialogo {
                    hablante = "Dra. Elena Vázquez (EAAF)",
                    cargo = "Antropóloga Forense Independiente - Equipo Argentino de Antropología Forense",
                    texto = "Bienvenida al laboratorio de confirmación, Esperanza. Gracias a que su brigada resguardó el área con banderines sin alterar el suelo en el Nivel 3, pudimos aplicar el protocolo forense internacional.",
                    notaEducativa = "DATO AUTODIDACTA: El EAAF es una organización científica no gubernamental que aplica la antropología y genética forense para investigar violaciones a los derechos humanos y restituir la identidad de personas desaparecidas."
                },
                new LineaDialogo {
                    hablante = "Esperanza (Madre Buscadora)",
                    cargo = "Fundadora del Colectivo Justicia y Esperanza",
                    texto = "Doctora, han pasado años de puertas cerradas y expedientes incompletos. Necesitamos la certeza científica de la verdad, por dolorosa que sea, para honrar su memoria con dignidad.",
                    notaEducativa = "CONTEXTO REAL: El 21 de marzo de 2011 salieron 22 migrantes de San Luis de la Paz rumbo a EE. UU.; su último rastro fue en la carretera hacia Dolores Hidalgo."
                },
                new LineaDialogo {
                    hablante = "Dra. Elena Vázquez (EAAF)",
                    cargo = "Antropóloga Forense Independiente - EAAF",
                    texto = "Hemos concluido el cruce genético multidisciplinario (ADN mitocondrial y nuclear, odontología y antropología física) respecto a los indicios recuperados en San Fernando, Tamaulipas.",
                    notaEducativa = "RIGOR CIENTÍFICO: La identificación forense requiere cotejar datos ante-mortem (señas particulares registradas en el Cuaderno de Búsqueda) con perfiles genéticos de los familiares."
                },
                new LineaDialogo {
                    hablante = "Dra. Elena Vázquez (EAAF)",
                    cargo = "Antropóloga Forense Independiente - EAAF",
                    texto = "Aquí tiene el Dictamen Forense Oficial bajo el expediente AP/PGR/SIEDO/UEITMIO/067/2012. Confirma con 99.99% de certeza la identidad de uno de los 22 compañeros, permitiendo una restitución digna a su familia.",
                    notaEducativa = "DERECHO A LA VERDAD: La entrega digna y documentada rompe el silencio institucional y obliga a mantener abiertas las líneas de investigación por los 21 restantes."
                },
                new LineaDialogo {
                    hablante = "Esperanza (Madre Buscadora)",
                    cargo = "Colectivo Justicia y Esperanza",
                    texto = "Lo guardo en nuestro Cuaderno de Búsqueda. Ahora iremos al Kiosco de la Plaza Central de San Luis de la Paz a colocar los 22 pañuelos bordados con hilo verde y encender las veladoras de la memoria.",
                    notaEducativa = "SIGUIENTE OBJETIVO: Dirígete a la derecha hacia el Kiosco/Mural de la Plaza para consagrar los 22 pañuelos verdes y encender el Altar de Veladoras."
                }
            };
        }

        public static List<LineaDialogo> ObtenerDialogoDonaTere()
        {
            return new List<LineaDialogo>
            {
                new LineaDialogo {
                    hablante = "Doña Tere",
                    cargo = "Compañera del Colectivo Justicia y Esperanza (Tía de dos jóvenes migrantes)",
                    texto = "Toma un poco de agua fresca, Esperanza. El camino desde la carretera a Dolores Hidalgo y las antesalas grises cansan el cuerpo, pero nunca el corazón cuando nos cuidamos entre todas.",
                    notaEducativa = "MECÁNICA DE SOLIDARIDAD: Compartir agua, palabras de aliento y el bordado colectivo fortalece la Línea de vida."
                },
                new LineaDialogo {
                    hablante = "Doña Tere",
                    cargo = "Compañera del Colectivo Justicia y Esperanza",
                    texto = "Recuerda a las madres y padres que fallecieron en el camino sin conocer la verdad. Por ellas, y por nuestros 22 hijos, bordamos con hilo verde cada nombre: el verde es la esperanza que no muere.",
                    notaEducativa = "MEMORIA COLECTIVA: Bordar en plazas públicas convierte el dolor privado en una denuncia pacífica, pedagógica y comunitaria."
                }
            };
        }

        public static PreguntaAutodidacta ObtenerPreguntaForense()
        {
            return new PreguntaAutodidacta
            {
                tituloFase = "Verificación Autodidacta: Protocolo Forense y Derechos Humanos",
                pregunta = "¿Por qué fue fundamental que la brigada ciudadana utilizara la Varilla 'T' y banderines de señalización sin excavar por su cuenta antes de la intervención del EAAF?",
                opciones = new string[]
                {
                    "Para preservar intacta la cadena de custodia, evitar contaminar los indicios genéticos y garantizar un dictamen científico con validez legal y trato digno.",
                    "Porque las familias no tenían interés en conocer la identidad de los hallazgos.",
                    "Para acelerar el cierre definitivo de todos los expedientes sin realizar pruebas de ADN."
                },
                indiceCorrecto = 0,
                explicacionCorrecta = "¡Excelente análisis! Preservar el contexto arqueológico y forense sin contaminar la escena permite que peritos independientes como el EAAF realicen identificaciones científicas irrefutables (+250 pts de Investigación).",
                explicacionIncorrecta = "Reflexión: En la búsqueda en campo, proteger el sitio con banderines sin alterar los restos es clave para no contaminar el ADN y asegurar la cadena de custodia científica."
            };
        }

        public static string ObtenerTextoActoFinalDocumental()
        {
            return "«El 21 de marzo de 2011, 22 hombres y jóvenes salieron de San Luis de la Paz, Guanajuato, buscando trabajo digno para sostener a sus familias.\n\n" +
                   "Frente al silencio burocrático y el retiro de apoyos institucionales, las mujeres de sus hogares fundaron el Colectivo Justicia y Esperanza, convirtiéndose en buscadoras de caminos, hospitales y desiertos.\n\n" +
                   "Con la intervención científica del Equipo Argentino de Antropología Forense (EAAF), la verdad comenzó a abrirse paso. Hoy, estas 22 veladoras y 22 pañuelos bordados con hilo verde iluminan la plaza pública.\n\n" +
                   "Exigimos a las autoridades mantener los apoyos a las familias víctimas indirectas, garantizar la búsqueda efectiva en vida y en campo, y no cerrar ningún expediente.\n\n" +
                   "PORQUE BUSCARLES ES UN ACTO DE AMOR.\n#14AñosBuscandoSLPZ   #HastaEncontrarles»";
        }

        public static string ObtenerTextoCreditosYLicencias()
        {
            return "=== CRÉDITOS OFICIALES DEL VIDEOJUEGO ===\n\n" +
                   "TÍTULO: Hasta Encontrarles: El Camino de la Esperanza (Nivel 4 Final)\n" +
                   "GÉNERO: Serious Game / Aventura Narrativa Documental 2D\n\n" +
                   "EQUIPO DE DESARROLLO (PARES):\n" +
                   "• María del Carmen Vargas Martínez — Lógica de Juego, Física 2D, Estados, Guardado y Mecánicas\n" +
                   "• Valeria Guadalupe Calvillo Mendoza — Dirección de Arte, UI/HUD, Narrativa Documental y Audio\n\n" +
                   "INSTITUCIÓN ACADÉMICA:\n" +
                   "• Universidad Tecnológica del Norte de Guanajuato (UTNG)\n" +
                   "• Ingeniería en Desarrollo y Gestión de Software — Grupo GIDS06001\n" +
                   "• Materia: Desarrollo de Videojuegos — Docente: Gabriel Barrón\n\n" +
                   "=== LICENCIAMIENTO Y FUENTES DE RECURSOS MULTIMEDIA ===\n" +
                   "1. Fotografía Documental y Memoria Histórica:\n" +
                   "   • Póster y retratos de los 22 desaparecidos de San Luis de la Paz (21 de marzo de 2011): Cortesía documental del Colectivo Justicia y Esperanza y Fundación para la Justicia (#14AñosBuscandoSLPZ), uso educativo y de memoria histórica sin fines de lucro.\n" +
                   "2. Arte 2D y Sprites:\n" +
                   "   • Sprite de Esperanza (Madre Buscadora con Varilla T y pico): Ilustración Pixel Art original del equipo de desarrollo procesada en hojas de animación 2D (Idle, Caminar, Salto, Acción).\n" +
                   "   • Escenarios 2D (bg_mural_memorial, Laboratorio EAAF, Kiosco de San Luis de la Paz) e Interfaz (ui_cuaderno_fondo, ui_caja_dialogo, pañuelos bordados): Creación original generada programáticamente para el proyecto (Licencia CC-BY-NC 4.0).\n" +
                   "3. Audio y Música (BGM / SFX):\n" +
                   "   • bgm_memorial_esperanza.wav y bgm_ruta_desierta.wav: Síntesis acústica original generada algorítmicamente en el motor (Dominio Público / CC0 1.0 Universal).\n" +
                   "   • Efectos de sonido (sfx_dialogo_blip, sfx_pasos_tierra, sfx_varilla_tierra, sfx_papel_hojear, sfx_veladora_encender, sfx_viento_plaza): Síntesis sonora procedimental libre de regalías (Licencia CC0 1.0).\n" +
                   "4. Motor y Herramientas:\n" +
                   "   • Unity 6 (6000.3.25f1) Universal Render Pipeline 2D (URP 2D).\n\n" +
                   "Dedicado con profundo respeto a las madres buscadoras de San Luis de la Paz y de todo México.";
        }
    }
}
