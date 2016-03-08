/**
Copyright (c) 2016 Ricardo Pistorello

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour {
    public static PoolManager sharedInstance;
	private Dictionary<string, Queue<GameObject> > queues;

	void Awake () {
        queues = new Dictionary<string, Queue<GameObject>> ();
        queues.Clear();

        //Singleton
        if (PoolManager.sharedInstance == null)
            PoolManager.sharedInstance = this;
        else if (PoolManager.sharedInstance != this)
            Destroy (gameObject);

        DontDestroyOnLoad(gameObject);
	}

    /** Set the max number of the instances. Cannot be reduced */
    public void clampMax(GameObject prefab, int toMax) {
        if (!queues.ContainsKey(prefab.name)) {
            allocate(prefab, toMax);
            return;
        }
        var queue = queues[prefab.name];
        int newMax = toMax - queue.Count;
        allocate(prefab, newMax);
    }

    /** Instantiate GameObjects */
    public void allocate(GameObject prefab, int number) {
        if (!queues.ContainsKey(prefab.name)) {
            queues.Add(prefab.name, new Queue<GameObject>());
        }
        for (int i = 0; i < number; i++) {
            GameObject newObj = (GameObject)Instantiate(prefab);
            newObj.name = prefab.name;
            newObj.SetActive(false);
            enqueue(newObj);

            if(newObj.GetComponent<PoolObject>() == null) newObj.AddComponent<PoolObject>();
        }
    }

    /** Put a GameObject into Queue. This method replaces DestroyObject()
     to improve memory management */
    public void enqueue(GameObject obj) {
		if(!queues.ContainsKey(obj.name))
            queues.Add(obj.name, new Queue<GameObject>());
        queues[obj.name].Enqueue(obj);
        obj.transform.SetParent(transform);
        obj.gameObject.SetActive(false);
    }


    /** Get a GameObject from Queue */
    public GameObject dequeue(GameObject prefab) {
        if(!queues.ContainsKey(prefab.name)) allocate(prefab, 1);
        Queue<GameObject> queue = queues[prefab.name];
        if(queue.Count <=0 ) allocate(prefab, 1);
        var dequeuedObject = queue.Dequeue();
        dequeuedObject.SetActive(true);
        return dequeuedObject;
    }


    /** Get a GameObject from Queue by its name. Return null if not found */
    public GameObject dequeueByName(string name) {
        if (!queues.ContainsKey(name)) return null;
        Queue<GameObject> queue = queues[name];
        if (queue.Count <= 0 ) return null;
        var dequeuedObject = queue.Dequeue();
        dequeuedObject.SetActive(true);
        return dequeuedObject;
    }
}
