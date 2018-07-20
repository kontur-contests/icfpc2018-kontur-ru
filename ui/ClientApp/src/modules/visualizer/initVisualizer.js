import * as THREE from "three";

export function initVisualizer(config, canvas, canvas_container) {
  const scale = 2;
  const dscale = 2 * scale;
  const hscale = scale / 2;
  const border = scale / 16;

  var sizeX;
  var sizeY;
  var sizeZ;
  var size;
  var hsizeX;
  var hsizeY;
  var hsizeZ;

  var loX;
  var hiX;
  var loY;
  var hiY;
  var loZ;
  var hiZ;

  var factor;

  function coordToPosX(x) {
    return factor * (x - hsizeX);
  }
  function coordToPosY(y) {
    return factor * (y - hsizeY);
  }
  function coordToPosZ(z) {
    return factor * (hsizeZ - z);
  }

  // Renderer
  var renderer = new THREE.WebGLRenderer({ canvas: canvas, antialias: true });
  renderer.setPixelRatio(window.devicePixelRatio);
  renderer.setClearColor(0x000000, 1);
  renderer.shadowMap.enabled = true;
  renderer.shadowMap.type = THREE.PCFShadowMap;
  // canvas_container.appendChild(renderer.domElement);

  // Camera
  var camera = new THREE.PerspectiveCamera(45, null, scale / 4, 4 * scale);
  camera.position.x = 0;
  camera.position.y = 0;
  camera.position.z = (9 / 8) * dscale;
  camera.lookAt(0, 0, 0);
  // var controls = new THREE.OrbitControls(camera);

  function onWindowResize() {
    canvas.style.removeProperty("height");
    canvas.style.removeProperty("width");
    const height = canvas.clientHeight;
    const width = canvas.clientWidth;
    camera.aspect = width / height;
    camera.updateProjectionMatrix();
    renderer.setSize(width, height);
  }
  window.addEventListener("resize", onWindowResize, false);
  onWindowResize();

  // Scene
  var scene = new THREE.Scene();

  // Lights
  var ambientLight = new THREE.AmbientLight(0x666666);
  scene.add(ambientLight);

  var pointLight = new THREE.PointLight(0xffffff, 1, 0);
  pointLight.position.set(-dscale, dscale, dscale);
  pointLight.castShadow = true;
  pointLight.shadow.bias = -0.0001;
  scene.add(pointLight);

  var pointLight = new THREE.PointLight(0x444444, 0.75, 0);
  pointLight.position.set(dscale, dscale, 2 * dscale);
  pointLight.castShadow = true;
  pointLight.shadow.bias = -0.0001;
  scene.add(pointLight);

  // Objects
  var objs = new THREE.Group();
  scene.add(objs);

  var floorMaterial = new THREE.MeshPhongMaterial({
    color: 0x888888,
    side: THREE.DoubleSide
  });
  var floorGeometry = new THREE.Geometry();
  var floor = new THREE.Mesh(floorGeometry, floorMaterial);
  floor.castShadow = false;
  floor.receiveShadow = true;
  objs.add(floor);
  function resizeFloor() {
    floorGeometry.vertices.length = 0;
    floorGeometry.faces.length = 0;
    floorGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(0), 0, coordToPosZ(0))
    );
    floorGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(0), 0, coordToPosZ(sizeZ))
    );
    floorGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(sizeX), 0, coordToPosZ(0))
    );
    floorGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(sizeX), 0, coordToPosZ(sizeZ))
    );
    floorGeometry.faces.push(new THREE.Face3(0, 1, 3));
    floorGeometry.faces.push(new THREE.Face3(0, 2, 3));
    floorGeometry.computeFaceNormals();
    floorGeometry.verticesNeedUpdate = true;
    floorGeometry.elementsNeedUpdate = true;
    floorGeometry.normalsNeedUpdate = true;
    floorGeometry.computeBoundingBox();
    floorGeometry.computeBoundingSphere();
    floor.position.y = coordToPosY(0);
  }

  var limitMaterial = new THREE.MeshPhongMaterial({
    color: 0xcccccc,
    side: THREE.DoubleSide
  });
  var limits = new THREE.Group();
  limits.visible = false;
  objs.add(limits);

  function resizeYLimit(yLimitGeometry) {
    yLimitGeometry.vertices.length = 0;
    yLimitGeometry.faces.length = 0;
    yLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(0), 0, coordToPosZ(0))
    );
    yLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(0), 0, coordToPosZ(sizeZ))
    );
    yLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(sizeX), 0, coordToPosZ(0))
    );
    yLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(sizeX), 0, coordToPosZ(sizeZ))
    );
    //yLimitGeometry.faces.push(new THREE.Face3(0, 1, 3));
    //yLimitGeometry.faces.push(new THREE.Face3(0, 2, 3));
    yLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(0) - border, 0, coordToPosZ(0) + border)
    );
    yLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(0) - border, 0, coordToPosZ(0))
    );
    yLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(0) - border, 0, coordToPosZ(sizeZ))
    );
    yLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(0) - border, 0, coordToPosZ(sizeZ) - border)
    );
    yLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(sizeX) + border, 0, coordToPosZ(0) + border)
    );
    yLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(sizeX) + border, 0, coordToPosZ(0))
    );
    yLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(sizeX) + border, 0, coordToPosZ(sizeZ))
    );
    yLimitGeometry.vertices.push(
      new THREE.Vector3(
        coordToPosX(sizeX) + border,
        0,
        coordToPosZ(sizeZ) - border
      )
    );
    yLimitGeometry.faces.push(new THREE.Face3(4, 8, 9));
    yLimitGeometry.faces.push(new THREE.Face3(4, 9, 5));
    yLimitGeometry.faces.push(new THREE.Face3(5, 0, 1));
    yLimitGeometry.faces.push(new THREE.Face3(5, 1, 6));
    yLimitGeometry.faces.push(new THREE.Face3(6, 10, 11));
    yLimitGeometry.faces.push(new THREE.Face3(6, 11, 7));
    yLimitGeometry.faces.push(new THREE.Face3(2, 9, 10));
    yLimitGeometry.faces.push(new THREE.Face3(2, 10, 3));
    yLimitGeometry.computeFaceNormals();
    yLimitGeometry.verticesNeedUpdate = true;
    yLimitGeometry.elementsNeedUpdate = true;
    yLimitGeometry.normalsNeedUpdate = true;
    yLimitGeometry.computeBoundingBox();
    yLimitGeometry.computeBoundingSphere();
  }
  var loYLimitGeometry = new THREE.Geometry();
  var loYLimit = new THREE.Mesh(loYLimitGeometry, limitMaterial);
  loYLimit.castShadow = false;
  loYLimit.receiveShadow = false;
  limits.add(loYLimit);
  function resizeLoYLimit() {
    resizeYLimit(loYLimitGeometry);
    loYLimit.position.y = coordToPosY(loY);
  }
  var hiYLimitGeometry = new THREE.Geometry();
  var hiYLimit = new THREE.Mesh(hiYLimitGeometry, limitMaterial);
  hiYLimit.castShadow = false;
  hiYLimit.receiveShadow = false;
  limits.add(hiYLimit);
  function resizeHiYLimit() {
    resizeYLimit(hiYLimitGeometry);
    hiYLimit.position.y = coordToPosY(hiY);
  }

  function resizeXLimit(xLimitGeometry) {
    xLimitGeometry.vertices.length = 0;
    xLimitGeometry.faces.length = 0;
    xLimitGeometry.vertices.push(
      new THREE.Vector3(0, coordToPosY(0), coordToPosZ(0))
    );
    xLimitGeometry.vertices.push(
      new THREE.Vector3(0, coordToPosY(0), coordToPosZ(sizeZ))
    );
    xLimitGeometry.vertices.push(
      new THREE.Vector3(0, coordToPosY(sizeY), coordToPosZ(0))
    );
    xLimitGeometry.vertices.push(
      new THREE.Vector3(0, coordToPosY(sizeY), coordToPosZ(sizeZ))
    );
    //xLimitGeometry.faces.push(new THREE.Face3(0, 1, 3));
    //xLimitGeometry.faces.push(new THREE.Face3(0, 2, 3));
    xLimitGeometry.vertices.push(
      new THREE.Vector3(0, coordToPosY(0) - border, coordToPosZ(0) + border)
    );
    xLimitGeometry.vertices.push(
      new THREE.Vector3(0, coordToPosY(0) - border, coordToPosZ(0))
    );
    xLimitGeometry.vertices.push(
      new THREE.Vector3(0, coordToPosY(0) - border, coordToPosZ(sizeZ))
    );
    xLimitGeometry.vertices.push(
      new THREE.Vector3(0, coordToPosY(0) - border, coordToPosZ(sizeZ) - border)
    );
    xLimitGeometry.vertices.push(
      new THREE.Vector3(0, coordToPosY(sizeY) + border, coordToPosZ(0) + border)
    );
    xLimitGeometry.vertices.push(
      new THREE.Vector3(0, coordToPosY(sizeY) + border, coordToPosZ(0))
    );
    xLimitGeometry.vertices.push(
      new THREE.Vector3(0, coordToPosY(sizeY) + border, coordToPosZ(sizeZ))
    );
    xLimitGeometry.vertices.push(
      new THREE.Vector3(
        0,
        coordToPosY(sizeY) + border,
        coordToPosZ(sizeZ) - border
      )
    );
    xLimitGeometry.faces.push(new THREE.Face3(4, 8, 9));
    xLimitGeometry.faces.push(new THREE.Face3(4, 9, 5));
    xLimitGeometry.faces.push(new THREE.Face3(5, 0, 1));
    xLimitGeometry.faces.push(new THREE.Face3(5, 1, 6));
    xLimitGeometry.faces.push(new THREE.Face3(6, 10, 11));
    xLimitGeometry.faces.push(new THREE.Face3(6, 11, 7));
    xLimitGeometry.faces.push(new THREE.Face3(2, 9, 10));
    xLimitGeometry.faces.push(new THREE.Face3(2, 10, 3));
    xLimitGeometry.computeFaceNormals();
    xLimitGeometry.verticesNeedUpdate = true;
    xLimitGeometry.elementsNeedUpdate = true;
    xLimitGeometry.normalsNeedUpdate = true;
    xLimitGeometry.computeBoundingBox();
    xLimitGeometry.computeBoundingSphere();
  }
  var loXLimitGeometry = new THREE.Geometry();
  var loXLimit = new THREE.Mesh(loXLimitGeometry, limitMaterial);
  loXLimit.castShadow = false;
  loXLimit.receiveShadow = false;
  limits.add(loXLimit);
  function resizeLoXLimit() {
    resizeXLimit(loXLimitGeometry);
    loXLimit.position.x = coordToPosX(loX);
  }
  var hiXLimitGeometry = new THREE.Geometry();
  var hiXLimit = new THREE.Mesh(hiXLimitGeometry, limitMaterial);
  hiXLimit.castShadow = false;
  hiXLimit.receiveShadow = false;
  limits.add(hiXLimit);
  function resizeHiXLimit() {
    resizeXLimit(hiXLimitGeometry);
    hiXLimit.position.x = coordToPosX(hiX);
  }

  function resizeZLimit(zLimitGeometry) {
    zLimitGeometry.vertices.length = 0;
    zLimitGeometry.faces.length = 0;
    zLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(0), coordToPosY(0), 0)
    );
    zLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(0), coordToPosY(sizeY), 0)
    );
    zLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(sizeX), coordToPosY(0), 0)
    );
    zLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(sizeX), coordToPosY(sizeY), 0)
    );
    // zLimitGeometry.faces.push(new THREE.Face3(0, 1, 3));
    // zLimitGeometry.faces.push(new THREE.Face3(0, 2, 3));
    zLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(0) - border, coordToPosY(0) - border, 0)
    );
    zLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(0) - border, coordToPosY(0), 0)
    );
    zLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(0) - border, coordToPosY(sizeY), 0)
    );
    zLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(0) - border, coordToPosY(sizeY) + border, 0)
    );
    zLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(sizeX) + border, coordToPosY(0) - border, 0)
    );
    zLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(sizeX) + border, coordToPosY(0), 0)
    );
    zLimitGeometry.vertices.push(
      new THREE.Vector3(coordToPosX(sizeX) + border, coordToPosY(sizeY), 0)
    );
    zLimitGeometry.vertices.push(
      new THREE.Vector3(
        coordToPosX(sizeX) + border,
        coordToPosY(sizeY) + border,
        0
      )
    );
    zLimitGeometry.faces.push(new THREE.Face3(4, 8, 9));
    zLimitGeometry.faces.push(new THREE.Face3(4, 9, 5));
    zLimitGeometry.faces.push(new THREE.Face3(5, 0, 1));
    zLimitGeometry.faces.push(new THREE.Face3(5, 1, 6));
    zLimitGeometry.faces.push(new THREE.Face3(6, 10, 11));
    zLimitGeometry.faces.push(new THREE.Face3(6, 11, 7));
    zLimitGeometry.faces.push(new THREE.Face3(2, 9, 10));
    zLimitGeometry.faces.push(new THREE.Face3(2, 10, 3));
    zLimitGeometry.computeFaceNormals();
    zLimitGeometry.verticesNeedUpdate = true;
    zLimitGeometry.elementsNeedUpdate = true;
    zLimitGeometry.normalsNeedUpdate = true;
    zLimitGeometry.computeBoundingBox();
    zLimitGeometry.computeBoundingSphere();
  }
  var loZLimitGeometry = new THREE.Geometry();
  var loZLimit = new THREE.Mesh(loZLimitGeometry, limitMaterial);
  loZLimit.castShadow = false;
  loZLimit.receiveShadow = false;
  limits.add(loZLimit);
  function resizeLoZLimit() {
    resizeZLimit(loZLimitGeometry);
    loZLimit.position.z = coordToPosZ(loZ);
  }
  var hiZLimitGeometry = new THREE.Geometry();
  var hiZLimit = new THREE.Mesh(hiZLimitGeometry, limitMaterial);
  hiZLimit.castShadow = false;
  hiZLimit.receiveShadow = false;
  limits.add(hiZLimit);
  function resizeHiZLimit() {
    resizeZLimit(hiZLimitGeometry);
    hiZLimit.position.z = coordToPosZ(hiZ);
  }

  function resizeLimits() {
    resizeLoYLimit();
    resizeHiYLimit();
    resizeLoXLimit();
    resizeHiXLimit();
    resizeLoZLimit();
    resizeHiZLimit();
  }

  var botMaterial = new THREE.MeshPhongMaterial({
    color: 0xbb8844,
    side: THREE.DoubleSide
  });
  var bots = new THREE.Group();
  objs.add(bots);
  function botMove(bot, x, y, z) {
    bot.position.x = coordToPosX(x + 0.5);
    bot.position.y = coordToPosY(y + 0.5);
    bot.position.z = coordToPosZ(z + 0.5);
  }
  function botAdd(x, y, z) {
    //var botGeometry = new THREE.BoxGeometry(scale/size, scale/size, scale/size);
    var botGeometry = new THREE.DodecahedronGeometry(hscale / size);
    //var botGeometry = new THREE.SphereGeometry(hscale/size);
    var bot = new THREE.Mesh(botGeometry, botMaterial);
    bot.castShadow = false;
    bot.receiveShadow = false;
    botMove(bot, x, y, z);
    bots.add(bot);
    return bot;
  }
  function botRem(bot) {
    bots.remove(bot);
  }

  var matrixMaterial = new THREE.MeshPhongMaterial({
    color: 0xffffff,
    side: THREE.DoubleSide
  });
  var matrixGeometry = new THREE.Geometry();
  var matrix = new THREE.Mesh(matrixGeometry, matrixMaterial);
  matrix.castShadow = true;
  matrix.receiveShadow = true;

  var matrixData = null;
  function getMatrix(x, y, z) {
    const i = x * sizeY * sizeZ + y * sizeZ + z;
    return matrixData[i] == 1;
  }
  function setMatrix(x, y, z, b) {
    const i = x * sizeY * sizeZ + y * sizeZ + z;
    matrixData[i] = b ? 1 : 0;
  }
  var matrixGeometryNeedsUpdate = false;
  function fillMatrix(lox, loy, loz, hix, hiy, hiz) {
    for (var x = lox; x <= hix; x++) {
      for (var y = loy; y <= hiy; y++) {
        for (var z = loz; z <= hiz; z++) {
          setMatrix(x, y, z, true);
        }
      }
    }
    matrixGeometryNeedsUpdate = true;
  }
  function voidMatrix(lox, loy, loz, hix, hiy, hiz) {
    for (var x = lox; x <= hix; x++) {
      for (var y = loy; y <= hiy; y++) {
        for (var z = loz; z <= hiz; z++) {
          setMatrix(x, y, z, false);
        }
      }
    }
    matrixGeometryNeedsUpdate = true;
  }
  function setMatrixFn(f) {
    for (var x = 0; x < sizeX; x++) {
      for (var y = 0; y < sizeY; y++) {
        for (var z = 0; z < sizeZ; z++) {
          setMatrix(x, y, z, f([x, y, z]));
        }
      }
    }
    matrixGeometryNeedsUpdate = true;
  }
  /*
    function getMatrix(x, y, z) {
        const i = x * sizeY * sizeZ + y * sizeZ + z;
        const q = i / 8 | 0;
        const r = i % 8;
        const w = matrixData[q];
        const b = 1 & (w >>> r);
        return (b == 1);
    };
    function setMatrix(x, y, z, b) {
        const i = x * sizeY * sizeZ + y * sizeZ + z;
        const q = i / 8 | 0;
        const r = i % 8;
        const w = matrixData[q];
        if (b) {
            matrixData[q] = w | (1 << r);
        } else {
            matrixData[q] = w & (~(1 << 1) && 255);
        }
    }
    */
  var matrixHasFace = false;
  function updateMatrixGeometry() {
    if (!matrixGeometryNeedsUpdate) {
      return;
    }
    var hasFace = false;
    function addQuad(x, y, z, dx1, dy1, dz1, dx2, dy2, dz2) {
      var n = matrixGeometry.vertices.length;
      function addVertex(x, y, z) {
        matrixGeometry.vertices.push(
          new THREE.Vector3(coordToPosX(x), coordToPosY(y), coordToPosZ(z))
        );
      }
      addVertex(x, y, z);
      addVertex(x + dx1, y + dy1, z + dz1);
      addVertex(x + dx2, y + dy2, z + dz2);
      addVertex(x + dx1 + dx2, y + dy1 + dy2, z + dz1 + dz2);
      matrixGeometry.faces.push(new THREE.Face3(n, n + 1, n + 3));
      matrixGeometry.faces.push(new THREE.Face3(n, n + 2, n + 3));
      hasFace = true;
    }
    matrixGeometry.vertices.length = 0;
    matrixGeometry.faces.length = 0;
    if (loX < hiX) {
      for (let x = loX; x <= hiX; x++) {
        for (let y = loY; y < hiY; y++) {
          for (let z = loZ; z < hiZ; z++) {
            function chk(z) {
              return (
                (x == loX && getMatrix(x, y, z)) ||
                (x == hiX && getMatrix(x - 1, y, z)) ||
                (x > loX &&
                  x < hiX &&
                  getMatrix(x, y, z) != getMatrix(x - 1, y, z))
              );
            }
            if (chk(z)) {
              var dz = 1;
              while (z + dz < hiZ && chk(z + dz)) {
                dz += 1;
              }
              addQuad(x, y, z, 0, 1, 0, 0, 0, dz);
              z += dz - 1;
            }
          }
        }
      }
    }
    if (loY < hiY) {
      for (let y = loY; y <= hiY; y++) {
        for (let x = loX; x < hiX; x++) {
          for (let z = loZ; z < hiZ; z++) {
            function chk(z) {
              return (
                (y > 0 && y == loY && getMatrix(x, y, z)) ||
                (y == hiY && getMatrix(x, y - 1, z)) ||
                (y > loY &&
                  y < hiY &&
                  getMatrix(x, y, z) != getMatrix(x, y - 1, z))
              );
            }
            if (chk(z)) {
              var dz = 1;
              while (z + dz < hiZ && chk(z + dz)) {
                dz += 1;
              }
              addQuad(x, y, z, 1, 0, 0, 0, 0, dz);
              z += dz - 1;
            }
          }
        }
      }
    }
    if (loZ < hiZ) {
      for (let z = loZ; z <= hiZ; z++) {
        for (let y = loY; y < hiY; y++) {
          for (let x = loX; x < hiX; x++) {
            function chk(x) {
              return (
                (z == loZ && getMatrix(x, y, z)) ||
                (z == hiZ && getMatrix(x, y, z - 1)) ||
                (z > loZ &&
                  z < hiZ &&
                  getMatrix(x, y, z) != getMatrix(x, y, z - 1))
              );
            }
            if (chk(x)) {
              var dx = 1;
              while (x + dx < hiX && chk(x + dx)) {
                dx += 1;
              }
              addQuad(x, y, z, dx, 0, 0, 0, 1, 0);
              x += dx - 1;
            }
          }
        }
      }
    }
    if (hasFace) {
      matrixGeometry.mergeVertices();
      matrixGeometry.computeFaceNormals();
      matrixGeometry.verticesNeedUpdate = true;
      matrixGeometry.elementsNeedUpdate = true;
      matrixGeometry.normalsNeedUpdate = true;
      matrixGeometry.computeBoundingBox();
      matrixGeometry.computeBoundingSphere();
      matrixGeometryNeedsUpdate = false;
      if (!matrixHasFace) {
        objs.add(matrix);
        matrixHasFace = true;
      }
    } else {
      if (matrixHasFace) {
        objs.remove(matrix);
        matrixHasFace = false;
      }
    }
  }

  function reset() {
    camera.position.z = (9 / 8) * dscale;
    camera.updateProjectionMatrix();
    objs.rotation.x = 0;
    objs.rotation.y = 0;
    limits.visible = false;
    loX = 0;
    loXLimit.position.x = coordToPosX(loX);
    hiX = sizeX;
    hiXLimit.position.x = coordToPosX(hiX);
    loY = 0;
    loYLimit.position.y = coordToPosY(loY);
    hiY = sizeY;
    hiYLimit.position.y = coordToPosY(hiY);
    loZ = 0;
    loZLimit.position.z = coordToPosZ(loZ);
    hiZ = sizeZ;
    hiZLimit.position.z = coordToPosZ(hiZ);
    matrixGeometryNeedsUpdate = true;
  }

  function setSize(sizeX_, sizeY_, sizeZ_) {
    sizeX = sizeX_;
    sizeY = sizeY_;
    sizeZ = sizeZ_;
    size = Math.max(sizeX, sizeY, sizeZ);
    hsizeX = sizeX / 2;
    hsizeY = sizeY / 2;
    hsizeZ = sizeZ / 2;
    loX = 0;
    loY = 0;
    loZ = 0;
    hiX = sizeX;
    hiY = sizeY;
    hiZ = sizeZ;

    factor = scale / size;

    reset();

    resizeFloor();
    resizeLimits();

    const dim = sizeX_ * sizeY_ * sizeZ_;
    matrixData = new Uint8Array(dim);
    matrixGeometryNeedsUpdate = true;

    objs.remove(bots);
    bots = new THREE.Group();
    objs.add(bots);
  }

  // // Stats
  // var stats;
  // if (config.stats) {
  //   stats = new Stats();
  //   canvas_container.appendChild(stats.dom);
  // }

  // Screenshot
  if (config.screenshot) {
    const b = document.createElement("button");
    b.innerHTML = "Screenshot";
    b.style.cssText =
      "position:absolute;top:0;right:0;cursor:pointer;z-index:10000";
    function screenshot() {
      var a = document.createElement("a");
      render();
      a.href = renderer.domElement.toDataURL("image/png");
      a.download = "screenshot.png";
      a.click();
    }
    b.onclick = screenshot;
    canvas_container.appendChild(b);
  }

  // Controls
  function onKeyPress(e) {
    if (e.key == "r") {
      reset();
    }

    // Guides
    if (e.key == "g") {
      limits.visible = !limits.visible;
    }

    // // Zooms
    // if (e.key == '1') {
    //     camera.position.z = 9/8 * dscale;
    //     camera.updateProjectionMatrix ();
    // }
    // if (e.key == '2') {
    //     camera.position.z = 8/8 * dscale;
    //     camera.updateProjectionMatrix ();
    // }
    // if (e.key == '3') {
    //     camera.position.z = 7/8 * dscale;
    //     camera.updateProjectionMatrix ();
    // }
    // if (e.key == '4') {
    //     camera.position.z = 6/8 * dscale;
    //     camera.updateProjectionMatrix ();
    // }
    // if (e.key == '5') {
    //     camera.position.z = 5/8 * dscale;
    //     camera.updateProjectionMatrix ();
    // }
    // if (e.key == '6') {
    //     camera.position.z = 4/8 * dscale;
    //     camera.updateProjectionMatrix ();
    // }
    // if (e.key == '7') {
    //     camera.position.z = 3/8 * dscale;
    //     camera.updateProjectionMatrix ();
    // }
    // if (e.key == '8') {
    //     camera.position.z = 2/8 * dscale;
    //     camera.updateProjectionMatrix ();
    // }
    // if (e.key == '9') {
    //     camera.position.z = 1/8 * dscale;
    //     camera.updateProjectionMatrix ();
    // }

    // Rotations
    const steps = 60;
    if (e.key == "w") {
      objs.rotation.x -= Math.PI / steps;
    }
    if (e.key == "s") {
      objs.rotation.x += Math.PI / steps;
    }
    if (e.key == "a") {
      objs.rotation.y -= Math.PI / steps;
    }
    if (e.key == "d") {
      objs.rotation.y += Math.PI / steps;
    }

    // X Limits
    if (e.key == "v") {
      if (loX > 0) {
        loX -= 1;
        loXLimit.position.x = coordToPosX(loX);
        for (let y = loY; y < hiY; y++) {
          for (let z = loZ; z < hiZ; z++) {
            if (getMatrix(loX, y, z)) {
              matrixGeometryNeedsUpdate = true;
              return;
            }
          }
        }
      }
    }
    if (e.key == "b") {
      if (loX < hiX) {
        loX += 1;
        loXLimit.position.x = coordToPosX(loX);
        for (let y = loY; y < hiY; y++) {
          for (let z = loZ; z < hiZ; z++) {
            if (getMatrix(loX - 1, y, z)) {
              matrixGeometryNeedsUpdate = true;
              return;
            }
          }
        }
      }
    }
    if (e.key == "n") {
      if (hiX > loX) {
        hiX -= 1;
        hiXLimit.position.x = coordToPosX(hiX);
        for (let y = loY; y < hiY; y++) {
          for (let z = loZ; z < hiZ; z++) {
            if (getMatrix(hiX, y, z)) {
              matrixGeometryNeedsUpdate = true;
              return;
            }
          }
        }
      }
    }
    if (e.key == "m") {
      if (hiX < sizeX) {
        hiX += 1;
        hiXLimit.position.x = coordToPosX(hiX);
        for (let y = loY; y < hiY; y++) {
          for (let z = loZ; z < hiZ; z++) {
            if (getMatrix(hiX - 1, y, z)) {
              matrixGeometryNeedsUpdate = true;
              return;
            }
          }
        }
      }
    }

    // Y Limits
    if (e.key == "h") {
      if (loY > 0) {
        loY -= 1;
        loYLimit.position.y = coordToPosY(loY);
        for (let x = loX; x < hiX; x++) {
          for (let z = loZ; z < hiZ; z++) {
            if (getMatrix(x, loY, z)) {
              matrixGeometryNeedsUpdate = true;
              return;
            }
          }
        }
      }
    }
    if (e.key == "j") {
      if (loY < hiY) {
        loY += 1;
        loYLimit.position.y = coordToPosY(loY);
        for (let x = loX; x < hiX; x++) {
          for (let z = loZ; z < hiZ; z++) {
            if (getMatrix(x, loY - 1, z)) {
              matrixGeometryNeedsUpdate = true;
              return;
            }
          }
        }
      }
    }
    if (e.key == "k") {
      if (hiY > loY) {
        hiY -= 1;
        hiYLimit.position.y = coordToPosY(hiY);
        for (let x = loX; x < hiX; x++) {
          for (let z = loZ; z < hiZ; z++) {
            if (getMatrix(x, hiY, z)) {
              matrixGeometryNeedsUpdate = true;
              return;
            }
          }
        }
      }
    }
    if (e.key == "l") {
      if (hiY < sizeY) {
        hiY += 1;
        hiYLimit.position.y = coordToPosY(hiY);
        for (let x = loX; x < hiX; x++) {
          for (let z = loZ; z < hiZ; z++) {
            if (getMatrix(x, hiY - 1, z)) {
              matrixGeometryNeedsUpdate = true;
              return;
            }
          }
        }
      }
    }

    // Z Limits
    if (e.key == "u") {
      if (loZ > 0) {
        loZ -= 1;
        loZLimit.position.z = coordToPosZ(loZ);
        for (let x = loX; x < hiX; x++) {
          for (let y = loY; y < hiY; y++) {
            if (getMatrix(x, y, loZ)) {
              matrixGeometryNeedsUpdate = true;
              return;
            }
          }
        }
      }
    }
    if (e.key == "i") {
      if (loZ < hiZ) {
        loZ += 1;
        loZLimit.position.z = coordToPosZ(loZ);
        for (let x = loX; x < hiX; x++) {
          for (let y = loY; y < hiY; y++) {
            if (getMatrix(x, y, loZ - 1)) {
              matrixGeometryNeedsUpdate = true;
              return;
            }
          }
        }
      }
    }
    if (e.key == "o") {
      if (hiZ > loZ) {
        hiZ -= 1;
        hiZLimit.position.z = coordToPosZ(hiZ);
        for (let x = loX; x < hiX; x++) {
          for (let y = loY; y < hiY; y++) {
            if (getMatrix(x, y, hiZ)) {
              matrixGeometryNeedsUpdate = true;
              return;
            }
          }
        }
      }
    }
    if (e.key == "p") {
      if (hiZ < sizeZ) {
        hiZ += 1;
        hiZLimit.position.z = coordToPosZ(hiZ);
        for (let x = loX; x < hiX; x++) {
          for (let y = loY; y < hiY; y++) {
            if (getMatrix(x, y, hiZ - 1)) {
              matrixGeometryNeedsUpdate = true;
              return;
            }
          }
        }
      }
    }
  }
  if (config.controls) {
    canvas.addEventListener("keypress", onKeyPress, false);
  }

  function render() {
    updateMatrixGeometry();
    renderer.render(scene, camera);
  }

  function animate() {
    requestAnimationFrame(animate);
    render();
  }

  setSize(8, 8, 8);
  animate();

  function removeSubscriptions() {
    window.removeEventListener("resize", onWindowResize, false);
    canvas.removeEventListener("keypress", onKeyPress, false);
  }

  return {
    setSize,
    setMatrixFn,
    fillMatrix,
    voidMatrix,
    botAdd,
    botRem,
    botMove,
    render,
    removeSubscriptions
  };
}
