using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;


public class PlayerNetwork : NetworkBehaviour
{
    
    [SerializeField] private float moveSpeed = 3f;

    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData
        {
            number = 10,
            bol = true,
        }, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    public struct MyCustomData: INetworkSerializable
    {
        public int number;
        public bool bol;
        public FixedString128Bytes message;
        
        private INetworkSerializable _networkSerializableImplementation;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref number);
            serializer.SerializeValue(ref bol);
            serializer.SerializeValue(ref message);
        }
    }
    
    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + "; randomNumber: " + newValue.number + "; " + newValue.bol
            + "; " + newValue.message
            );
        };
    }

    private void Update()
    {
        
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            // randomNumber.Value = new MyCustomData
            // {
            //     number = Random.Range(0,10),
            //     bol = false,
            //     message = (Random.Range(0,100)).ToString(),
            // };
            TestServerRpc(new ServerRpcParams());
            TestClientRpc(
                new ClientRpcParams {Send = new ClientRpcSendParams
                {
                    TargetClientIds = new List<ulong>{ 1 }
                }}
                );
        }
        
        Vector3 moveDir = new Vector3(0, 0, 0);
        
        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        transform.position += moveDir * moveSpeed * Time.deltaTime;

    }

    [ServerRpc]
    private void TestServerRpc(ServerRpcParams serverRpcParams)
    {
        Debug.Log("TestServerRpc "+ OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void TestClientRpc(ClientRpcParams clientRpcParams)
    {
        Debug.Log("TestClientRpc: ");
    }
}
