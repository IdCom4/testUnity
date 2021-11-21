using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
  private Transform cam;
  private World world;
  public Text selectedBlockText;
  [SerializeField] public GameObject highlightBlock;
  [SerializeField] public float highlightBlockUpScale = 0.001f;
  Vector3 placeBlockPos = new Vector3();

  public bool isGrounded;
  public bool isSprinting;

  public float walkSpeed = 5f;
  public float sprintSpeed = 10f;
  public float sensitivity = 1.5f;
  public float jumpForce = 0.015f;
  public float gravity = -0.1f;

  public float playerMinus = 0.1f;
  public float playerWidthRadius = 0.3f;
  public float playerHeight = 1.8f;
  public float checkIncrement = 0.1f;
  public float reach = 8f;
  public byte reachedBlockID;
  public Vector3 reachedBlockPosition;

  private float horizontal;
  private float vertical;
  private float mouseHorizontal;
  private float mouseVertical;

  private Vector3 velocity;
  private float verticalMomentum = 0f;
  private bool jumpRequest;

  private float timeElasped;

  public Toolbar toolbar;

  private void Start() {
    cam = GameObject.Find("Main Camera").transform;
    world = GameObject.Find("World").GetComponent<World>();

    Vector3 newHighlightBlockScale = new Vector3(highlightBlockUpScale, highlightBlockUpScale, highlightBlockUpScale);

    Cursor.lockState = CursorLockMode.Locked;

    highlightBlock.transform.localScale += newHighlightBlockScale;
  }

  private void Update() {

    if (Input.GetKeyDown(KeyCode.I)) {
      world.inUI = !world.inUI;
    }

    if (!world.inUI) {
      GetPlayerInputs();
    
      CalculateVelocity();

      
      transform.Rotate(Vector3.up * mouseHorizontal * sensitivity);
      cam.Rotate(Vector3.right * -mouseVertical * sensitivity);
      

      if (jumpRequest)
        Jump();
      transform.Translate(velocity, Space.World);

      PointBlock();
    }
    
  }

  public void PointBlock() {
    float step = checkIncrement;
    Vector3 lastPos = new Vector3();
  
    while (step < reach) {
      Vector3 pos = cam.position + (cam.forward * step);
      int x = Mathf.FloorToInt(pos.x);
      int y = Mathf.FloorToInt(pos.y);
      int z = Mathf.FloorToInt(pos.z);

      byte blockID = world.CheckBlockIdAt(pos);
      BlockType bt = VoxelData.blockTypes[blockID];

      if (bt != null && bt.isSolid) {
        float xHightlight = x - (highlightBlock.transform.localScale.x / 2) + (highlightBlockUpScale / 2) + 1;
        float yHightlight = y - (highlightBlock.transform.localScale.y / 2) + (highlightBlockUpScale / 2) + 1;
        float zHightlight = z - (highlightBlock.transform.localScale.z / 2) + (highlightBlockUpScale / 2) + 1;

        highlightBlock.transform.position = new Vector3(xHightlight, yHightlight, zHightlight);
        highlightBlock.gameObject.SetActive(true);
        placeBlockPos = lastPos;

        reachedBlockID = blockID;
        reachedBlockPosition = new Vector3(x, y, z);
        return ;
      }

      lastPos = new Vector3(x, y, z);
      step += checkIncrement;
    }

    reachedBlockID = BlockID.AIR;
    
    if (highlightBlock.gameObject.activeSelf) {
      highlightBlock.gameObject.SetActive(false);
    }
  }

  void Jump() {
    verticalMomentum = jumpForce;
    isGrounded = false;
    jumpRequest = false;
  }

  private void CalculateVelocity() {
    
    // if we're sprinting, use the sprint multiplier
    float moveSpeed = isSprinting ? sprintSpeed : walkSpeed;
    velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.deltaTime * moveSpeed;

    if (isGrounded)
    {
      verticalMomentum = 0;
      timeElasped = 0.1f;
    }

    timeElasped += Time.deltaTime;
    velocity.y = timeElasped * timeElasped * gravity + verticalMomentum;

    if (velocity.x < 0)
      velocity.x = CheckXminusSpeed(velocity.x);
    else if (velocity.x > 0)
      velocity.x = CheckXplusSpeed(velocity.x);

    if (velocity.z < 0)
      velocity.z = CheckZminusSpeed(velocity.z);
    else if (velocity.z > 0)
      velocity.z = CheckZplusSpeed(velocity.z);

    if (velocity.y < 0)
      velocity.y = CheckDownSpeed(velocity.y);
    else if (velocity.y > 0)
      velocity.y = CheckUpSpeed(velocity.y);
  }

  private void GetPlayerInputs() {
    horizontal = Input.GetAxis("Horizontal");
    vertical = Input.GetAxis("Vertical");
    mouseHorizontal = Input.GetAxis("Mouse X");
    mouseVertical = Input.GetAxis("Mouse Y");
  
    // sprint
    if (Input.GetButtonDown("Sprint"))
      isSprinting = true;
    if (Input.GetButtonUp("Sprint"))
      isSprinting = false;
    
    // jump
    if (isGrounded && Input.GetButtonDown("Jump"))
      jumpRequest = true;

    // destroy and place block
    if (reachedBlockID != BlockID.AIR) {
      if (Input.GetMouseButtonDown(0)) {
        Chunk reachedBlockChunk = world.GetChunkFromVector3(reachedBlockPosition);
        if (reachedBlockChunk != null)
          reachedBlockChunk.EditBlock(reachedBlockPosition, BlockID.AIR);
      }
      if (Input.GetMouseButtonDown(1)) {
        int pX = Mathf.FloorToInt(transform.position.x);
        int pY = Mathf.FloorToInt(transform.position.y);
        int pZ = Mathf.FloorToInt(transform.position.z);

        int bX = Mathf.FloorToInt(placeBlockPos.x);
        int bY = Mathf.FloorToInt(placeBlockPos.y);
        int bZ = Mathf.FloorToInt(placeBlockPos.z);

        if (bX != pX || (bY != pY && bY != pY + 1) || bZ != pZ) {
          Chunk blockPlaceChunk = world.GetChunkFromVector3(placeBlockPos);
          if (blockPlaceChunk != null) {
            if (toolbar.slots[toolbar.slotIndex].hasItem) {
              blockPlaceChunk.EditBlock(placeBlockPos, toolbar.slots[toolbar.slotIndex].itemSlot.stack.id);
              toolbar.slots[toolbar.slotIndex].itemSlot.Take(1);
            }
          }
        } 
      }
    }
  }

  private float CheckDownSpeed(float downSpeed) {
    BlockType downBlock1 = world.CheckBlockTypeAt(new Vector3(transform.position.x - playerWidthRadius, transform.position.y + downSpeed, transform.position.z - playerWidthRadius));
    BlockType downBlock2 = world.CheckBlockTypeAt(new Vector3(transform.position.x + playerWidthRadius, transform.position.y + downSpeed, transform.position.z - playerWidthRadius));
    BlockType downBlock3 = world.CheckBlockTypeAt(new Vector3(transform.position.x + playerWidthRadius, transform.position.y + downSpeed, transform.position.z + playerWidthRadius));
    BlockType downBlock4 = world.CheckBlockTypeAt(new Vector3(transform.position.x - playerWidthRadius, transform.position.y + downSpeed, transform.position.z + playerWidthRadius));
    if (
      (downBlock1 != null && downBlock1.isSolid) ||
      (downBlock2 != null && downBlock2.isSolid) ||
      (downBlock3 != null && downBlock3.isSolid) ||
      (downBlock4 != null && downBlock4.isSolid)
    ) {
      isGrounded = true;
      return (0);
    } else {
      isGrounded = false;
      return (downSpeed);
    }
  }

  private float CheckUpSpeed(float upSpeed) {
    BlockType upBlock1 = world.CheckBlockTypeAt(new Vector3(transform.position.x - playerWidthRadius, transform.position.y + 2f + upSpeed, transform.position.z - playerWidthRadius));
    BlockType upBlock2 = world.CheckBlockTypeAt(new Vector3(transform.position.x + playerWidthRadius, transform.position.y + 2f + upSpeed, transform.position.z - playerWidthRadius));
    BlockType upBlock3 = world.CheckBlockTypeAt(new Vector3(transform.position.x + playerWidthRadius, transform.position.y + 2f + upSpeed, transform.position.z + playerWidthRadius));
    BlockType upBlock4 = world.CheckBlockTypeAt(new Vector3(transform.position.x - playerWidthRadius, transform.position.y + 2f + upSpeed, transform.position.z + playerWidthRadius));
    if (
      (upBlock1 != null && upBlock1.isSolid) ||
      (upBlock2 != null && upBlock2.isSolid) ||
      (upBlock3 != null && upBlock3.isSolid) ||
      (upBlock4 != null && upBlock4.isSolid)
    )
      return (0);
    else
      return (upSpeed);
  }

  public float CheckZplusSpeed(float speed) {
    BlockType frontLeftBottomBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x - (playerWidthRadius + playerMinus), transform.position.y, transform.position.z + (playerWidthRadius + playerMinus) + speed));
    BlockType frontLeftTopBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x - (playerWidthRadius + playerMinus), transform.position.y + 1f, transform.position.z + (playerWidthRadius + playerMinus) + speed));
    
    BlockType frontRightBottomBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x + (playerWidthRadius + playerMinus), transform.position.y, transform.position.z + (playerWidthRadius + playerMinus) + speed));
    BlockType frontRightTopBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x + (playerWidthRadius + playerMinus), transform.position.y + 1f, transform.position.z + (playerWidthRadius + playerMinus) + speed));
    
    if (
      (frontLeftBottomBlock != null && frontLeftBottomBlock.isSolid) ||
      (frontLeftTopBlock != null && frontLeftTopBlock.isSolid) ||
      (frontRightBottomBlock != null && frontRightBottomBlock.isSolid) ||
      (frontRightTopBlock != null && frontRightTopBlock.isSolid)
    )
      return 0;
    else
      return speed;
  }
  
  public float CheckZminusSpeed(float speed) {
    BlockType backLeftBottomBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x - (playerWidthRadius + playerMinus), transform.position.y, transform.position.z - (playerWidthRadius + playerMinus) + speed));
    BlockType backLeftTopBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x - (playerWidthRadius + playerMinus), transform.position.y + 1f, transform.position.z - (playerWidthRadius + playerMinus) + speed));
    
    BlockType backRightBottomBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x + (playerWidthRadius + playerMinus), transform.position.y, transform.position.z - (playerWidthRadius + playerMinus) + speed));
    BlockType backRightTopBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x + (playerWidthRadius + playerMinus), transform.position.y + 1f, transform.position.z - (playerWidthRadius + playerMinus) + speed));
    
    if (
      (backLeftBottomBlock != null && backLeftBottomBlock.isSolid) ||
      (backLeftTopBlock != null && backLeftTopBlock.isSolid) ||
      (backRightBottomBlock != null && backRightBottomBlock.isSolid) ||
      (backRightTopBlock != null && backRightTopBlock.isSolid)
    )
      return 0;
    else
      return speed;
  }

  public float CheckXminusSpeed(float speed) {
    BlockType leftBackBottomBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x - (playerWidthRadius + playerMinus) + speed, transform.position.y, transform.position.z - (playerWidthRadius + playerMinus)));
    BlockType leftBackTopBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x - (playerWidthRadius + playerMinus) + speed, transform.position.y + 1f, transform.position.z - (playerWidthRadius + playerMinus)));
    
    BlockType leftFrontBottomBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x - (playerWidthRadius + playerMinus) + speed, transform.position.y, transform.position.z + (playerWidthRadius + playerMinus)));
    BlockType leftFrontTopBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x - (playerWidthRadius + playerMinus) + speed, transform.position.y + 1f, transform.position.z + (playerWidthRadius + playerMinus)));
    
    if (
      (leftBackBottomBlock != null && leftBackBottomBlock.isSolid) ||
      (leftBackTopBlock != null && leftBackTopBlock.isSolid) ||
      (leftFrontBottomBlock != null && leftFrontBottomBlock.isSolid) ||
      (leftFrontTopBlock != null && leftFrontTopBlock.isSolid)
    )
      return 0;
    else
      return speed;
  }

  public float CheckXplusSpeed(float speed) {
    BlockType rightBackBottomBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x + (playerWidthRadius + playerMinus) + speed, transform.position.y, transform.position.z - (playerWidthRadius + playerMinus)));
    BlockType rightBackTopBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x + (playerWidthRadius + playerMinus) + speed, transform.position.y + 1f, transform.position.z - (playerWidthRadius + playerMinus)));
    
    BlockType rightFrontBottomBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x + (playerWidthRadius + playerMinus) + speed, transform.position.y, transform.position.z + (playerWidthRadius + playerMinus)));
    BlockType rightFrontTopBlock = world.CheckBlockTypeAt(new Vector3(transform.position.x + (playerWidthRadius + playerMinus) + speed, transform.position.y + 1f, transform.position.z + (playerWidthRadius + playerMinus)));
    
    if (
      (rightBackBottomBlock != null && rightBackBottomBlock.isSolid) ||
      (rightBackTopBlock != null && rightBackTopBlock.isSolid) ||
      (rightFrontBottomBlock != null && rightFrontBottomBlock.isSolid) ||
      (rightFrontTopBlock != null && rightFrontTopBlock.isSolid)
    )
      return 0;
    else
      return speed;
  }
}
