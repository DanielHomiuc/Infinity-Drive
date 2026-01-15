using System.Collections;
using UnityEngine;

public class EndlessLevelHandler:MonoBehaviour
{

    [SerializeField]
    GameObject[] sectionsPrefabs;

    // all sections from the game
    GameObject[] sectionsPool = new GameObject[20];

    // will select from the pool and put them here and render them
    GameObject[] sections = new GameObject[10];

    Transform playerCarTransform;

    WaitForSeconds waitFor100ms = new WaitForSeconds(0.1f);

    const float sectionLength = 26;

    void Start()
    {
        // detect where the car is in the game world
        playerCarTransform = GameObject.FindGameObjectWithTag("Player").transform;

        int prefabIndex = 0;


        // crate a pool for our endless sections
        for(int i = 0; i < sectionsPool.Length; i++)
        {
            sectionsPool[i] = Instantiate(sectionsPrefabs[prefabIndex]);
            // start everything disabled we dont want to see the pool now
            sectionsPool[i].SetActive(false);

            prefabIndex++;

            // loop the prefab index if we run out of prefabs
            if(prefabIndex > sectionsPrefabs.Length - 1)
            {
                prefabIndex = 0;
            }
        }

        // add the first sections to the road
        for (int i = 0; i < sections.Length; i++)
        {
            // get random section
            GameObject randomSection = GetRandomSectionFromPool();

            // move it into position and activate the section
            randomSection.transform.position = new Vector3(sectionsPool[i].transform.position.x, -10, i * sectionLength);
            randomSection.SetActive(true);

            sections[i] = randomSection;
        }

        StartCoroutine(UpdateLessOftenCO());
    }

    IEnumerator UpdateLessOftenCO()
    {
        while(true)
        {
            UpdateSectionPositions();
            yield return waitFor100ms;
        }
    }

    void UpdateSectionPositions()
    {
        for(int i = 0; i < sections.Length; i++)
        {
            // check if the section is too far behind
            if(sections[i].transform.position.z - playerCarTransform.position.z < -sectionLength)
            {
                //Store posotion of the section but disable it
                Vector3 lastSectionPosition = sections[i].transform.position;
                sections[i].SetActive(false);

                // get new section & enable it and move it forward
                sections[i] = GetRandomSectionFromPool();

                // move it from behind of the player until the head of the player
                sections[i].transform.position = new Vector3(lastSectionPosition.x, -10, lastSectionPosition.z + sectionLength * sections.Length);
                sections[i].SetActive(true);
            }
        }
    }

    GameObject GetRandomSectionFromPool()
    {
        int randomIndex = Random.Range(0, sectionsPool.Length);

        bool isNewSectionFound = false;

        while(!isNewSectionFound)
        {
            // check if the section is not active, if not then found a section
            if (!sectionsPool[randomIndex].activeInHierarchy)
            {
                isNewSectionFound = true;
            } else
            {
                // try to find another section
                randomIndex++;

                // if we reach the end then start random again
                if(randomIndex > sectionsPool.Length - 1)
                {
                    randomIndex = 0;
                }
            }
        }

        return sectionsPool[randomIndex];
    }
}
