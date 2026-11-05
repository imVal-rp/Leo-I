using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Historia y objetivos de cada escena.</summary>
public static class Guion
{
    private const float SueloPueblo = -3.75f;
    private const float SueloBosque = -3.95f;
    private const float SueloHacienda = -3.41f;

    private static readonly Color FondoMenu = new Color(0.07f, 0.05f, 0.09f, 1f);
    private static readonly Color FondoGameOver = new Color(0.12f, 0.02f, 0.03f, 1f);
    private static readonly Color FondoVictoria = new Color(0.12f, 0.08f, 0.05f, 1f);

    private class Aldeano
    {
        public string Nombre;
        public Transform Lugar;
        public string Texto;
        public bool Hablo;
    }

    private static Juego J => Juego.I;
    private static Hud H => Juego.I.Hud;

    public static IEnumerator ParaEscena(string escena)
    {
        switch (escena)
        {
            case Juego.EscenaMenu: return Menu();
            case "pueblo": return Pueblo();
            case "Bosque": return Bosque();
            case "Hacienda_patio": return Patio();
            case "Hacienda_pasillo": return Pasillo();
            case "Hacienda_final": return BatallaFinal();
            case Juego.EscenaGameOver: return GameOver();
            case Juego.EscenaVictoria: return Victoria();
            default: return Vacio();
        }
    }

    private static IEnumerator Vacio()
    {
        yield break;
    }

    private static Bounds CamaraDe(float izquierda, float derecha, float abajo, float arriba)
    {
        Vector3 centro = new Vector3((izquierda + derecha) / 2f, (abajo + arriba) / 2f, 0f);
        Vector3 tamano = new Vector3(derecha - izquierda, arriba - abajo, 1f);
        return new Bounds(centro, tamano);
    }

    private static IEnumerator Hablar(string nombre, string texto)
    {
        bool hayLeo = J.LeoMovimiento != null;

        if (hayLeo)
            J.LeoMovimiento.PuedeMoverse = false;

        yield return H.Dialogo(nombre, texto);

        if (hayLeo)
            J.LeoMovimiento.PuedeMoverse = true;
    }

    private static IEnumerator Esperar(float segundos)
    {
        yield return new WaitForSeconds(segundos);
    }

    private static IEnumerator LlegarA(float x)
    {
        while (J.LeoTransform != null && J.LeoTransform.position.x < x)
            yield return null;
    }

    private static IEnumerator Desvanecer(SpriteRenderer sprite, float desde, float hasta, float duracion)
    {
        Color color = sprite.color;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            color.a = Mathf.Lerp(desde, hasta, tiempo / duracion);
            sprite.color = color;
            yield return null;
        }

        color.a = hasta;
        sprite.color = color;
    }

    private static bool PresionoInteractuar()
    {
        Keyboard teclado = Keyboard.current;
        return teclado != null && teclado.eKey.wasPressedThisFrame;
    }

    // ---------- Pantallas ----------

    private static IEnumerator Menu()
    {
        H.MostrarPantalla(
            "LEO I",
            "ENTER para jugar\n\n" +
            "A / D: caminar     W: saltar\n" +
            "J: atacar     K: protegerse\n" +
            "E: hablar o recoger     Espacio: continuar\n\n" +
            "ESC para salir",
            FondoMenu,
            Hojas.Cuadro("teodora_limpio", "0"),
            Hojas.Cuadro("nahuala_limpio", "0"));

        while (true)
        {
            Keyboard teclado = Keyboard.current;

            if (teclado != null && (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame))
            {
                J.SiguienteEscena();
                yield break;
            }

            if (teclado != null && teclado.escapeKey.wasPressedThisFrame)
                Application.Quit();

            yield return null;
        }
    }

    private static IEnumerator GameOver()
    {
        Sonido.Reproducir("derrota");

        H.MostrarPantalla(
            "GAME OVER",
            "El miedo venció a Leo...\n\nENTER para reintentar\nESC para volver al menú",
            FondoGameOver);

        while (true)
        {
            Keyboard teclado = Keyboard.current;

            if (teclado != null && (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame))
            {
                J.Reintentar();
                yield break;
            }

            if (teclado != null && teclado.escapeKey.wasPressedThisFrame)
            {
                J.IrA(Juego.EscenaMenu);
                yield break;
            }

            yield return null;
        }
    }

    private static IEnumerator Victoria()
    {
        Sonido.Reproducir("victoria");

        H.MostrarPantalla(
            "VICTORIA",
            "Leo superó su miedo y La Nahuala perdió su poder.\n" +
            "Teodora está a salvo y por fin pueden volver a casa.\n\n" +
            "Sánchez Carrasco Yoosavi Adriana\n" +
            "Rodríguez Ponce Valeria Aurora\n" +
            "Osorio Rojas José Mauricio\n\n" +
            "ENTER para volver al menú",
            FondoVictoria,
            Hojas.Cuadro("teodora_limpio", "0"));

        while (true)
        {
            Keyboard teclado = Keyboard.current;

            if (teclado != null && (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame))
            {
                J.IrA(Juego.EscenaMenu);
                yield break;
            }

            yield return null;
        }
    }

    // ---------- Acto I: pueblo ----------

    private static IEnumerator Pueblo()
    {
        J.PrepararNivel(SueloPueblo, -22.5f, 22.4f, CamaraDe(-23f, 23f, -5f, 5f), -12f, false);

        // La carreta bloquea la salida al bosque hasta después del secuestro.
        const float barreraX = 19.3f;
        GameObject barrera = new GameObject("BarreraSalida");
        Juego.CrearCaja(barrera, new Vector2(barreraX, J.Suelo + 4f), new Vector2(1f, 10f));

        GameObject carreta = GameObject.Find("carreta_0");

        if (carreta != null)
            carreta.transform.position = new Vector3(barreraX + 1.5f, carreta.transform.position.y, 0f);

        AnimadorSprite teodora = Fabrica.CrearTeodora(new Vector2(-9.8f, J.Suelo));
        teodora.Mirar(false);

        List<Aldeano> aldeanos = new List<Aldeano>
        {
            CrearAldeano("hombre_joven_0", "Joven", "Dicen que su guarida está en lo profundo del bosque."),
            CrearAldeano("anciano_0", "Anciano", "Hace años, todos temían a La Nahuala. Dicen que se alimenta del miedo."),
            CrearAldeano("mujer_0", "Señora", "No le hagas caso, son puros cuentos.")
        };

        yield return H.Letrero("Te mudaste con tu familia a Salsipuedes,\nun pequeño pueblo del norte de México.");
        yield return Hablar("Teodora", "Vamos a conocer el pueblo, Leo. Dicen que aquí cuentan historias muy raras.");

        yield return HablarConTodos(aldeanos, "Habla con la gente del pueblo");

        yield return Hablar("Leo", "¿Y si La Nahuala es real...?");

        // Esa noche.
        J.LeoMovimiento.PuedeMoverse = false;
        yield return H.Fundir(1f, 0.7f);

        H.Noche(true);
        H.Objetivo(string.Empty);

        Vector3 posicionLeo = J.LeoTransform.position;
        posicionLeo.x = 1.5f;
        J.LeoTransform.position = posicionLeo;
        teodora.transform.position = new Vector3(4.8f, J.Suelo, 0f);

        AnimadorSprite nahuala = Fabrica.CrearNahualaDeEscena(new Vector2(6.6f, J.Suelo));
        nahuala.Mirar(false);
        nahuala.Render.color = new Color(1f, 1f, 1f, 0f);

        yield return H.Letrero("Esa noche...");
        yield return H.Fundir(0f, 0.7f);

        yield return Hablar("Teodora", "¿Leo? Hay alguien allá afuera...");

        Sonido.Reproducir("invocar");
        yield return Desvanecer(nahuala.Render, 0f, 1f, 1.2f);

        yield return Hablar("La Nahuala", "Puedo oler tu miedo, muchacho. Gracias a él, volví.");
        yield return Hablar("Teodora", "¡Leo, ayúdame!");

        Sonido.Reproducir("sombra");
        J.Hud.StartCoroutine(Desvanecer(teodora.Render, 1f, 0f, 1.2f));
        yield return Desvanecer(nahuala.Render, 1f, 0f, 1.2f);

        Object.Destroy(teodora.gameObject);
        Object.Destroy(nahuala.gameObject);

        yield return Hablar("Leo", "¡TEODORA!");

        aldeanos[0].Texto = "Fue un mal sueño. Vuelve a dormir.";
        aldeanos[1].Texto = "Solo fue tu imaginación, muchacho.";
        aldeanos[2].Texto = "¿La Nahuala? Ya te dije que son puros cuentos.";

        foreach (Aldeano aldeano in aldeanos)
            aldeano.Hablo = false;

        yield return HablarConTodos(aldeanos, "Pide ayuda a la gente del pueblo");

        yield return Hablar("Leo", "Nadie me cree... Entonces iré solo. Su guarida está en el bosque.");

        Object.Destroy(barrera);

        if (carreta != null)
        {
            SpriteRenderer spriteCarreta = carreta.GetComponent<SpriteRenderer>();

            if (spriteCarreta != null)
                yield return Desvanecer(spriteCarreta, 1f, 0f, 0.6f);

            carreta.SetActive(false);
        }

        H.Objetivo("Busca a Teodora: sal hacia el bosque  >>");

        yield return LlegarA(21.2f);
        J.SiguienteEscena();
    }

    private static Aldeano CrearAldeano(string objeto, string nombre, string texto)
    {
        GameObject encontrado = GameObject.Find(objeto);

        if (encontrado == null)
            Debug.LogWarning($"No se encontró el personaje '{objeto}' en la escena.");

        return new Aldeano
        {
            Nombre = nombre,
            Lugar = encontrado != null ? encontrado.transform : null,
            Texto = texto,
            Hablo = encontrado == null
        };
    }

    private static IEnumerator HablarConTodos(List<Aldeano> aldeanos, string objetivo)
    {
        while (true)
        {
            int faltan = 0;
            Aldeano cercano = null;
            float mejor = 2.2f;

            foreach (Aldeano aldeano in aldeanos)
            {
                if (aldeano.Hablo || aldeano.Lugar == null)
                    continue;

                faltan++;

                float distancia = Mathf.Abs(aldeano.Lugar.position.x - J.LeoTransform.position.x);

                if (distancia < mejor)
                {
                    mejor = distancia;
                    cercano = aldeano;
                }
            }

            if (faltan == 0)
            {
                H.OcultarIndicador();
                yield break;
            }

            H.Objetivo($"{objetivo} ({aldeanos.Count - faltan}/{aldeanos.Count})");

            if (cercano != null)
            {
                H.MostrarIndicador(cercano.Lugar.position + Vector3.up * 2.2f, "E: hablar");

                if (PresionoInteractuar())
                {
                    H.OcultarIndicador();
                    yield return Hablar(cercano.Nombre, cercano.Texto);
                    cercano.Hablo = true;
                }
            }
            else
            {
                H.OcultarIndicador();
            }

            yield return null;
        }
    }

    // ---------- Acto II: bosque ----------

    private static IEnumerator Bosque()
    {
        J.PrepararNivel(SueloBosque, -9.99f, 10.2f, CamaraDe(-10.11f, 10.26f, -5.05f, 5.75f), -8.8f, false);

        // Eran objetos de prueba de la escena.
        DestruirSiExiste("nahuala_limpio_0");
        DestruirSiExiste("chupacabras_limpio_0");

        H.Objetivo("Busca a Teodora");
        yield return Hablar("Leo", "El Bosque de las Sombras... La guarida debe estar por aquí.");

        // Machete.
        SpriteRenderer machete = Fabrica.CrearMachete(new Vector2(-3f, J.Suelo + 0.4f));
        H.Objetivo("Revisa lo que brilla en el camino");

        while (true)
        {
            if (Mathf.Abs(J.LeoTransform.position.x - machete.transform.position.x) < 1.5f)
            {
                H.MostrarIndicador(machete.transform.position + Vector3.up * 1.2f, "E: recoger");

                if (PresionoInteractuar())
                    break;
            }
            else
            {
                H.OcultarIndicador();
            }

            yield return null;
        }

        H.OcultarIndicador();
        Object.Destroy(machete.gameObject);
        Sonido.Reproducir("machete");

        yield return Hablar("Leo", "Un machete viejo y roto... No importa. Con esto rescataré a Teodora.");
        yield return Hablar("Leo", "Si de verdad soy un caballero, este machete será mi espada de energía.");

        J.Leo.DarMachete();

        yield return H.Letrero("¡Obtuviste el machete de energía!\nJ: atacar     K: protegerse     W: saltar");

        H.Objetivo("Sobrevive a las criaturas del bosque");
        yield return Hablar("Leo", "¿Qué es eso? ¡Algo sale de la tierra!");

        yield return Oleada(new[] { false, false });
        yield return Esperar(0.8f);
        yield return Oleada(new[] { true, false, true });

        yield return Esperar(0.6f);
        yield return Hablar("Leo", "Protegen el camino... Debo estar cerca de la guarida.");

        H.Objetivo("Sigue hacia la hacienda  >>");
        yield return LlegarA(9.2f);
        J.SiguienteEscena();
    }

    private static void DestruirSiExiste(string nombre)
    {
        GameObject objeto = GameObject.Find(nombre);

        if (objeto != null)
            Object.Destroy(objeto);
    }

    private static IEnumerator Oleada(bool[] calaveras)
    {
        List<Enemigo> enemigos = new List<Enemigo>();

        for (int i = 0; i < calaveras.Length; i++)
        {
            float lado = i % 2 == 0 ? 1f : -1f;
            float x = J.LeoTransform.position.x + lado * (5f + i);
            x = Mathf.Clamp(x, J.LimiteIzquierdo + 1f, J.LimiteDerecho - 1f);

            Vector2 pies = new Vector2(x, J.Suelo);
            enemigos.Add(calaveras[i] ? (Enemigo)Fabrica.CrearCalavera(pies) : Fabrica.CrearMomia(pies));
        }

        while (enemigos.Exists(enemigo => enemigo != null && enemigo.Vivo))
            yield return null;
    }

    // ---------- Acto III: hacienda ----------

    private static IEnumerator Patio()
    {
        J.PrepararNivel(SueloHacienda, -9.7f, 9.7f, CamaraDe(-10f, 10f, -5f, 5f), -8f, true);

        H.Objetivo("Hacienda “Ya Merito”");
        yield return Hablar("Leo", "La hacienda “Ya Merito”... Parece abandonada.");

        Sonido.Reproducir("grito");
        yield return Hablar("???", "¡Ay, mis hijos!");
        yield return Hablar("Leo", "Esa voz... ¡Es La Llorona!");

        LloronaEnemiga llorona = Fabrica.CrearLlorona(new Vector2(5f, J.Suelo));
        yield return PelearConJefe(llorona, "LA LLORONA", "Bloquea su grito con K o sáltalo con W");

        yield return Esperar(1.2f);
        H.Objetivo("Entra por la puerta  >>");
        yield return LlegarA(7.6f);
        J.SiguienteEscena();
    }

    private static IEnumerator Pasillo()
    {
        J.PrepararNivel(SueloHacienda, -9.7f, 9.7f, CamaraDe(-10f, 10f, -5f, 5f), -8f, true);

        Sonido.Reproducir("rugido");
        yield return Hablar("Leo", "¿Qué fue ese chillido? Las leyendas de Salsipuedes son reales...");

        Chupacabras chupacabras = Fabrica.CrearChupacabras(new Vector2(6f, J.Suelo));
        yield return PelearConJefe(chupacabras, "EL CHUPACABRAS", "Bloquea su embestida con K y golpéalo cuando se detenga");

        yield return Esperar(1.2f);
        yield return Hablar("Leo", "Estas criaturas nacen de mis miedos. Así se fortalece La Nahuala.");

        H.Objetivo("Corre hacia la puerta  >>");
        yield return LlegarA(7.6f);
        J.SiguienteEscena();
    }

    private static IEnumerator PelearConJefe(Enemigo jefe, string nombre, string consejo)
    {
        H.MostrarJefe(nombre);
        H.Objetivo(consejo);
        jefe.AlCambiarVida += () => H.ActualizarJefe(jefe.PorcentajeVida);

        while (jefe != null && jefe.Vivo)
            yield return null;

        H.OcultarJefe();
        H.Objetivo(string.Empty);
    }

    private static IEnumerator BatallaFinal()
    {
        J.PrepararNivel(SueloHacienda, -9.7f, 9.7f, CamaraDe(-10f, 10f, -5f, 5f), -6.5f, true);

        AnimadorSprite teodora = Fabrica.CrearTeodora(new Vector2(7.8f, J.Suelo));
        teodora.Mirar(false);

        Nahuala nahuala = Fabrica.CrearNahuala(new Vector2(3.5f, J.Suelo));
        nahuala.GetComponent<AnimadorSprite>().Mirar(false);

        yield return Hablar("Leo", "¡Teodora!");
        yield return Hablar("Teodora", "¡Leo! ¡Ten cuidado, es una trampa!");
        yield return Hablar("La Nahuala", "Tu miedo siempre fue la fuente de mi poder. Ella solo era el camino para traerte hasta aquí.");
        yield return Hablar("Leo", "Ya no voy a huir. ¡Suelta a mi hermana!");

        nahuala.Empezar();
        yield return PelearConJefe(nahuala, "LA NAHUALA", "Derrota a sus criaturas y golpéala cuando quede aturdida");

        nahuala.EliminarCriaturas();
        yield return Esperar(2.4f);

        yield return Hablar("Teodora", "¡Lo lograste, Leo!");
        yield return Hablar("Leo", "Ser un caballero no depende del arma. Depende del valor para protegerte. Vamos a casa.");

        J.IrA(Juego.EscenaVictoria);
    }
}
