using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ControladorIA : MonoBehaviour
{
    public enum Estados
    {
        Esperar,
        Patrulhar,
        Perseguir,
        Procurar
    }

    public Estados estadoAtual;

    //movimento da IA
    private NavMeshAgent agent;
    private Transform alvo;

    [Header("Estado: Esperar")]
    public float tempoEsperar = 0f;
    private float tempoEsperando;

    [Header("Estado: Patrulhar")]
    public Transform waypoint1;
    public Transform waypoint2;
    private Transform waypointAtual;
    public float distanciaMinWaypoint = 2;
    private float distanciaWaypointAtual;

    [Header("Estado: Perseguir")]
    public GameObject alvoInicial;
    public GameObject alvoSecundario;
    public Transform alvoPrincipal;
    private float distanciaJogador;
    public float campoVisao = 5;

    [Header("Estado: Procurar")]
    public float tempoPersistencia = 0f;
    private float tempoSemVisao;

    [Header("Ataque")]
    public Transform pontoAtaque;
    public Transform pontoAtaqueDois;
    public float areaAtaque = .5f;
    public LayerMask layerMask;

    [Header("Ataque com impulso")]
    private float direcaoX = 0;
    private float direcaoZ = 0;
    public float forcaHorizontal;
    public float forcaVertical;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        waypointAtual = waypoint1;
        Esperar();
    }
    
    void Update()
    {
        if (alvoPrincipal.position.x <= transform.position.x)
        {
            direcaoX = 1;
        }
        if (alvoPrincipal.position.x > transform.position.x)
        {
            direcaoX = -1;
        }

        if (alvoPrincipal.position.z <= transform.position.z)
        {
            direcaoZ = 1;
        }
        if (alvoPrincipal.position.z > transform.position.z)
        {
            direcaoZ = -1;
        }

        ChecarEstados();
        Atacar();
    }

    private void Esperar()
    {
        estadoAtual = Estados.Esperar;
        tempoEsperando = Time.time;
    }

    private void ChecarEstados()
    {

        if (estadoAtual != Estados.Perseguir && PossuiVisaoJogador())
        {
            Perseguir();
        }

        switch (estadoAtual)
        {
            case Estados.Esperar:
                
                if (esperouTempoSuficiente())
                {
                    Patrulhar();
                }
                else
                {
                    alvo = transform;
                }

                break;

            case Estados.Patrulhar:

                if (PertoWaypointAtual())
                {
                    Esperar();
                    AlterarWaypoint();
                }
                else
                {
                    alvo = waypointAtual;
                }

                break;

            case Estados.Perseguir:

                if (!PossuiVisaoJogador())
                {
                    Procurar();
                }
                else
                {
                    alvo = alvoPrincipal;
                }

                break;

            case Estados.Procurar:

                if (SemVisaoTempoSuficiente())
                {
                    Esperar();
                }

                break;
        }

        if (alvoInicial == null)
        {
            alvoInicial = GameObject.FindWithTag("Player");
        }

        if(alvoSecundario == null)
        {
            alvoSecundario = GameObject.FindWithTag("Player");
        }

        //Define o alvo escolhido
        agent.SetDestination(alvo.position);
    }

    private void Atacar()
    {
        Collider[] inimigosBatidos = Physics.OverlapSphere(pontoAtaque.position, areaAtaque, layerMask);

        foreach (var inimigo in inimigosBatidos)
        {
            if (alvoPrincipal.position == alvoSecundario.transform.position)
            {
                //invoca a função de ataque sofrido no inimigo para sofrer a repulsão
                alvoSecundario.GetComponent<ControladorIA>().ataqueSofrido();
            }
        }

        Collider[] jogadorBatido = Physics.OverlapSphere(pontoAtaqueDois.position, areaAtaque, layerMask);

        foreach (var jogador in jogadorBatido)
        {
            //print($"Bati no {jogador.name}!");

            if (alvoPrincipal.position == alvoInicial.transform.position)
            {
                //invoca a função de ataque sofrido no inimigo para sofrer a repulsão
            }
        }
    }

    private bool SemVisaoTempoSuficiente()
    {
        return tempoSemVisao + tempoPersistencia <= Time.time;
    }

    private void Procurar()
    {
        estadoAtual = Estados.Procurar;
        tempoSemVisao = Time.time;
        alvo = null;
        //procurarOutroJogador();
    }

    private void procurarOutroJogador()
    {
        if (alvo == null) 
        {
            alvoPrincipal = null;

            int selectEnemy = UnityEngine.Random.Range(1,3);

            // estava dentro do if "selectEnemy == 1 && !alvoPrincipal == alvoInicial.transform"
            if (Vector3.Distance(transform.position, alvoInicial.transform.position) <= campoVisao)
            {
                //alvoPrincipal = GameObject.FindGameObjectWithTag("Player").transform;
                alvoPrincipal = alvoInicial.transform;
                
                if (alvoInicial == null)
                    procurarOutroJogador();
            }

            //estava dentro do if "selectEnemy == 2 && !alvoPrincipal == alvoSecundario.transform"
            else if (Vector3.Distance(transform.position, alvoSecundario.transform.position) <= campoVisao)
            {
                //alvoPrincipal = GameObject.FindGameObjectWithTag("IA").transform;
                alvoPrincipal = alvoSecundario.transform;
                
                if (alvoSecundario == null)
                    procurarOutroJogador();
            }
        }
    }

    private void Perseguir()
    {
        estadoAtual = Estados.Perseguir;
    }

    private bool PossuiVisaoJogador()
    {
        if (Vector3.Distance(transform.position, alvoInicial.transform.position) <= campoVisao)
        {
            alvoPrincipal = alvoInicial.transform;
        }

        if (Vector3.Distance(transform.position, alvoSecundario.transform.position) <= campoVisao)
        {
            alvoPrincipal = alvoSecundario.transform;
        }

        distanciaJogador = Vector3.Distance(transform.position, alvoPrincipal.position);

        return distanciaJogador <= campoVisao;
    }

    private void AlterarWaypoint()
    {
        waypointAtual = (waypointAtual == waypoint1)? waypoint2 : waypoint1;   
    }

    private bool PertoWaypointAtual()
    {
        distanciaWaypointAtual = Vector3.Distance(transform.position, waypointAtual.position);
        return distanciaWaypointAtual <= distanciaMinWaypoint;
    }

    private void Patrulhar()
    {
        estadoAtual = Estados.Patrulhar;
    }

    private bool esperouTempoSuficiente()
    {
        return tempoEsperando + tempoEsperar <= Time.time;
    }

    private void OnDrawGizmosSelected()
    {
        if (pontoAtaque == null)
            return;

        Gizmos.DrawWireSphere(pontoAtaque.position, areaAtaque);
        Gizmos.DrawWireSphere(pontoAtaqueDois.position, areaAtaque);
    }

    void ataqueSofrido()
    {
        //print("Sofri ataque");
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().useGravity = true;
        agent.enabled = false;
        GetComponent<Rigidbody>().velocity = new Vector3(direcaoX * forcaHorizontal, forcaVertical, direcaoZ * forcaVertical);
        StartCoroutine(Delay());
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(1f);
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().useGravity = true;
        agent.enabled = true;
    }
}
