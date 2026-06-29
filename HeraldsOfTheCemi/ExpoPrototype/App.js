import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
  Pressable,
  SafeAreaView,
  StatusBar,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { GLView } from 'expo-gl';

const WORLD_LIMIT = 28;
const PLAYER_SPEED = 7.5;

const MISSIONS = [
  {
    title: 'Paint the CEMI mural',
    hint: 'Move to the violet beacon beside the wall, then press TAG.',
    action: 'tag',
    target: [-15, 10],
    reward: 250,
  },
  {
    title: 'Win the streetball challenge',
    hint: 'Reach the orange court and press BALL.',
    action: 'ball',
    target: [13, -12],
    reward: 400,
  },
  {
    title: 'Protect the block',
    hint: 'Reach the red danger beacon and press POWER.',
    action: 'power',
    target: [0, 18],
    reward: 750,
  },
];

const COLORS = {
  sky: [0.025, 0.045, 0.11],
  ground: [0.075, 0.12, 0.12],
  road: [0.08, 0.09, 0.12],
  sidewalk: [0.28, 0.3, 0.34],
  player: [0.1, 0.75, 1.0],
  playerDark: [0.02, 0.25, 0.55],
  skin: [0.48, 0.25, 0.15],
  violet: [0.75, 0.12, 1.0],
  orange: [1.0, 0.35, 0.05],
  red: [1.0, 0.08, 0.12],
  gold: [1.0, 0.75, 0.08],
};

function clamp(value, min, max) {
  return Math.max(min, Math.min(max, value));
}

function distance2D(a, b) {
  const dx = a[0] - b[0];
  const dz = a[2] - b[1];
  return Math.sqrt(dx * dx + dz * dz);
}

function normalize3(v) {
  const length = Math.hypot(v[0], v[1], v[2]) || 1;
  return [v[0] / length, v[1] / length, v[2] / length];
}

function cross3(a, b) {
  return [
    a[1] * b[2] - a[2] * b[1],
    a[2] * b[0] - a[0] * b[2],
    a[0] * b[1] - a[1] * b[0],
  ];
}

function subtract3(a, b) {
  return [a[0] - b[0], a[1] - b[1], a[2] - b[2]];
}

function mat4Identity() {
  return new Float32Array([
    1, 0, 0, 0,
    0, 1, 0, 0,
    0, 0, 1, 0,
    0, 0, 0, 1,
  ]);
}

function mat4Multiply(a, b) {
  const out = new Float32Array(16);
  for (let column = 0; column < 4; column += 1) {
    for (let row = 0; row < 4; row += 1) {
      out[column * 4 + row] =
        a[0 * 4 + row] * b[column * 4 + 0] +
        a[1 * 4 + row] * b[column * 4 + 1] +
        a[2 * 4 + row] * b[column * 4 + 2] +
        a[3 * 4 + row] * b[column * 4 + 3];
    }
  }
  return out;
}

function mat4Perspective(fovRadians, aspect, near, far) {
  const f = 1 / Math.tan(fovRadians / 2);
  const rangeInv = 1 / (near - far);
  return new Float32Array([
    f / aspect, 0, 0, 0,
    0, f, 0, 0,
    0, 0, (near + far) * rangeInv, -1,
    0, 0, near * far * rangeInv * 2, 0,
  ]);
}

function mat4LookAt(eye, target, up) {
  const zAxis = normalize3(subtract3(eye, target));
  const xAxis = normalize3(cross3(up, zAxis));
  const yAxis = cross3(zAxis, xAxis);

  return new Float32Array([
    xAxis[0], yAxis[0], zAxis[0], 0,
    xAxis[1], yAxis[1], zAxis[1], 0,
    xAxis[2], yAxis[2], zAxis[2], 0,
    -(xAxis[0] * eye[0] + xAxis[1] * eye[1] + xAxis[2] * eye[2]),
    -(yAxis[0] * eye[0] + yAxis[1] * eye[1] + yAxis[2] * eye[2]),
    -(zAxis[0] * eye[0] + zAxis[1] * eye[1] + zAxis[2] * eye[2]),
    1,
  ]);
}

function mat4Compose(position, scale, rotationY = 0) {
  const c = Math.cos(rotationY);
  const s = Math.sin(rotationY);
  return new Float32Array([
    c * scale[0], 0, -s * scale[0], 0,
    0, scale[1], 0, 0,
    s * scale[2], 0, c * scale[2], 0,
    position[0], position[1], position[2], 1,
  ]);
}

function createCubeGeometry(gl) {
  const positions = new Float32Array([
    -0.5, -0.5, 0.5, 0.5, -0.5, 0.5, 0.5, 0.5, 0.5, -0.5, 0.5, 0.5,
    0.5, -0.5, -0.5, -0.5, -0.5, -0.5, -0.5, 0.5, -0.5, 0.5, 0.5, -0.5,
    -0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, -0.5, -0.5, 0.5, -0.5,
    -0.5, -0.5, -0.5, 0.5, -0.5, -0.5, 0.5, -0.5, 0.5, -0.5, -0.5, 0.5,
    0.5, -0.5, 0.5, 0.5, -0.5, -0.5, 0.5, 0.5, -0.5, 0.5, 0.5, 0.5,
    -0.5, -0.5, -0.5, -0.5, -0.5, 0.5, -0.5, 0.5, 0.5, -0.5, 0.5, -0.5,
  ]);

  const normals = new Float32Array([
    0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1,
    0, 0, -1, 0, 0, -1, 0, 0, -1, 0, 0, -1,
    0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0,
    0, -1, 0, 0, -1, 0, 0, -1, 0, 0, -1, 0,
    1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0,
    -1, 0, 0, -1, 0, 0, -1, 0, 0, -1, 0, 0,
  ]);

  const indices = new Uint16Array([
    0, 1, 2, 0, 2, 3,
    4, 5, 6, 4, 6, 7,
    8, 9, 10, 8, 10, 11,
    12, 13, 14, 12, 14, 15,
    16, 17, 18, 16, 18, 19,
    20, 21, 22, 20, 22, 23,
  ]);

  const positionBuffer = gl.createBuffer();
  gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);
  gl.bufferData(gl.ARRAY_BUFFER, positions, gl.STATIC_DRAW);

  const normalBuffer = gl.createBuffer();
  gl.bindBuffer(gl.ARRAY_BUFFER, normalBuffer);
  gl.bufferData(gl.ARRAY_BUFFER, normals, gl.STATIC_DRAW);

  const indexBuffer = gl.createBuffer();
  gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indexBuffer);
  gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, indices, gl.STATIC_DRAW);

  return { positionBuffer, normalBuffer, indexBuffer, count: indices.length };
}

function compileShader(gl, type, source) {
  const shader = gl.createShader(type);
  gl.shaderSource(shader, source);
  gl.compileShader(shader);
  if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
    throw new Error(gl.getShaderInfoLog(shader) || 'Shader compilation failed');
  }
  return shader;
}

function createProgram(gl) {
  const vertexShader = compileShader(
    gl,
    gl.VERTEX_SHADER,
    `
      precision mediump float;
      attribute vec3 aPosition;
      attribute vec3 aNormal;
      uniform mat4 uMVP;
      uniform mat4 uModel;
      uniform vec3 uColor;
      varying vec3 vColor;
      varying float vDepth;

      void main() {
        vec3 worldNormal = normalize(mat3(uModel) * aNormal);
        vec3 lightDirection = normalize(vec3(0.35, 0.9, 0.25));
        float diffuse = max(dot(worldNormal, lightDirection), 0.0);
        float light = 0.35 + diffuse * 0.65;
        vColor = uColor * light;
        vec4 clipPosition = uMVP * vec4(aPosition, 1.0);
        vDepth = clipPosition.z / clipPosition.w;
        gl_Position = clipPosition;
      }
    `,
  );

  const fragmentShader = compileShader(
    gl,
    gl.FRAGMENT_SHADER,
    `
      precision mediump float;
      varying vec3 vColor;
      varying float vDepth;

      void main() {
        float fog = smoothstep(0.25, 1.0, vDepth);
        vec3 fogColor = vec3(0.025, 0.045, 0.11);
        gl_FragColor = vec4(mix(vColor, fogColor, fog * 0.72), 1.0);
      }
    `,
  );

  const program = gl.createProgram();
  gl.attachShader(program, vertexShader);
  gl.attachShader(program, fragmentShader);
  gl.linkProgram(program);
  if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
    throw new Error(gl.getProgramInfoLog(program) || 'Program linking failed');
  }
  return program;
}

function buildCity() {
  const buildings = [];
  const palette = [
    [0.16, 0.22, 0.32],
    [0.28, 0.16, 0.2],
    [0.2, 0.28, 0.25],
    [0.3, 0.24, 0.16],
    [0.18, 0.17, 0.28],
  ];
  const coordinates = [-24, -16, -8, 8, 16, 24];

  coordinates.forEach((x, xIndex) => {
    coordinates.forEach((z, zIndex) => {
      if (Math.abs(x) < 7 || Math.abs(z) < 7) return;
      if ((x === -16 && z === 8) || (x === 16 && z === -16)) return;
      const seed = Math.abs(xIndex * 17 + zIndex * 31 + x * 3 - z * 2);
      const height = 4.5 + (seed % 10);
      buildings.push({
        position: [x, height / 2, z],
        scale: [5.8, height, 5.8],
        color: palette[seed % palette.length],
      });
    });
  });
  return buildings;
}

function drawWorld(renderer, game) {
  const { drawCube } = renderer;
  const { player, missionIndex, time, buildings } = game;

  drawCube([0, -0.65, 0], [62, 1, 62], COLORS.ground);
  drawCube([0, -0.08, 0], [62, 0.12, 7.5], COLORS.road);
  drawCube([0, -0.07, 0], [7.5, 0.14, 62], COLORS.road);
  drawCube([0, 0, 6.2], [62, 0.12, 1.3], COLORS.sidewalk);
  drawCube([0, 0, -6.2], [62, 0.12, 1.3], COLORS.sidewalk);
  drawCube([6.2, 0, 0], [1.3, 0.12, 62], COLORS.sidewalk);
  drawCube([-6.2, 0, 0], [1.3, 0.12, 62], COLORS.sidewalk);

  for (let i = -24; i <= 24; i += 6) {
    drawCube([i, 0.02, 0], [2.6, 0.04, 0.14], COLORS.gold);
    drawCube([0, 0.02, i], [0.14, 0.04, 2.6], COLORS.gold);
  }

  buildings.forEach((building) => {
    drawCube(building.position, building.scale, building.color);
    const roofY = building.position[1] + building.scale[1] / 2 + 0.15;
    drawCube(
      [building.position[0], roofY, building.position[2]],
      [building.scale[0] * 0.82, 0.25, building.scale[2] * 0.82],
      [0.08, 0.1, 0.15],
    );
  });

  drawCube([-17.5, 2.1, 10], [0.65, 4.2, 8], [0.38, 0.2, 0.36]);
  drawCube([-17.1, 2.2, 10], [0.1, 2.4, 5.5], COLORS.violet);
  drawCube([-16.98, 2.2, 10], [0.08, 1.5, 3.8], COLORS.orange);

  drawCube([13, 0.03, -12], [11, 0.08, 9], [0.45, 0.16, 0.06]);
  drawCube([13, 0.08, -12], [0.14, 0.03, 8], [0.95, 0.8, 0.52]);
  drawCube([13, 0.08, -12], [10, 0.03, 0.14], [0.95, 0.8, 0.52]);
  drawCube([17.4, 2.4, -12], [0.18, 4.8, 0.18], COLORS.sidewalk);
  drawCube([17, 4.35, -12], [0.18, 2.4, 2.1], [0.85, 0.86, 0.9]);
  drawCube([16.75, 3.75, -12], [0.15, 0.15, 1.0], COLORS.orange);

  [-2.5, 0, 2.5].forEach((x, index) => {
    const bob = Math.sin(time * 2.1 + index) * 0.1;
    drawCube([x, 1.2 + bob, 20], [0.9, 1.7, 0.9], COLORS.red, index * 0.5);
    drawCube([x, 2.45 + bob, 20], [0.65, 0.65, 0.65], [0.3, 0.05, 0.05]);
  });

  if (missionIndex < MISSIONS.length) {
    const mission = MISSIONS[missionIndex];
    const pulse = 1 + Math.sin(time * 4) * 0.22;
    const beaconColor = missionIndex === 0 ? COLORS.violet : missionIndex === 1 ? COLORS.orange : COLORS.red;
    drawCube([mission.target[0], 1.65, mission.target[1]], [0.7 * pulse, 3.3, 0.7 * pulse], beaconColor);
    drawCube([mission.target[0], 3.7, mission.target[1]], [1.4 * pulse, 0.16, 1.4 * pulse], COLORS.gold);
  }

  const bob = Math.sin(time * 8) * 0.05;
  drawCube([player[0], 1.2 + bob, player[2]], [0.9, 1.5, 0.6], COLORS.player, game.playerYaw);
  drawCube([player[0], 2.35 + bob, player[2]], [0.68, 0.68, 0.68], COLORS.skin, game.playerYaw);
  drawCube([player[0], 0.35 + bob, player[2] - 0.22], [0.3, 0.9, 0.3], COLORS.playerDark, game.playerYaw);
  drawCube([player[0], 0.35 + bob, player[2] + 0.22], [0.3, 0.9, 0.3], COLORS.playerDark, game.playerYaw);
  drawCube([player[0] - 0.58, 1.35 + bob, player[2]], [0.25, 1.15, 0.25], COLORS.player, game.playerYaw);
  drawCube([player[0] + 0.58, 1.35 + bob, player[2]], [0.25, 1.15, 0.25], COLORS.player, game.playerYaw);
  drawCube([player[0], 1.55 + bob, player[2] + 0.36], [0.46, 0.55, 0.08], COLORS.gold, game.playerYaw);
}

function GameGL({ controlsRef, missionRef, playerRef, onReady, onError }) {
  const buildings = useMemo(() => buildCity(), []);

  const onContextCreate = useCallback(
    (gl) => {
      try {
        const program = createProgram(gl);
        const cube = createCubeGeometry(gl);
        const locations = {
          position: gl.getAttribLocation(program, 'aPosition'),
          normal: gl.getAttribLocation(program, 'aNormal'),
          mvp: gl.getUniformLocation(program, 'uMVP'),
          model: gl.getUniformLocation(program, 'uModel'),
          color: gl.getUniformLocation(program, 'uColor'),
        };

        gl.useProgram(program);
        gl.enable(gl.DEPTH_TEST);
        gl.enable(gl.CULL_FACE);
        gl.cullFace(gl.BACK);
        gl.clearColor(...COLORS.sky, 1);

        gl.bindBuffer(gl.ARRAY_BUFFER, cube.positionBuffer);
        gl.enableVertexAttribArray(locations.position);
        gl.vertexAttribPointer(locations.position, 3, gl.FLOAT, false, 0, 0);

        gl.bindBuffer(gl.ARRAY_BUFFER, cube.normalBuffer);
        gl.enableVertexAttribArray(locations.normal);
        gl.vertexAttribPointer(locations.normal, 3, gl.FLOAT, false, 0, 0);
        gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, cube.indexBuffer);

        const projection = mat4Perspective(
          Math.PI / 3.1,
          gl.drawingBufferWidth / gl.drawingBufferHeight,
          0.1,
          130,
        );

        let view = mat4Identity();
        const renderer = {
          drawCube(position, scale, color, rotationY = 0) {
            const model = mat4Compose(position, scale, rotationY);
            const viewModel = mat4Multiply(view, model);
            const mvp = mat4Multiply(projection, viewModel);
            gl.uniformMatrix4fv(locations.model, false, model);
            gl.uniformMatrix4fv(locations.mvp, false, mvp);
            gl.uniform3fv(locations.color, color);
            gl.drawElements(gl.TRIANGLES, cube.count, gl.UNSIGNED_SHORT, 0);
          },
        };

        let lastTime = 0;
        let playerYaw = Math.PI;

        const render = (timestamp) => {
          const seconds = timestamp / 1000;
          const dt = Math.min(0.035, Math.max(0, seconds - lastTime));
          lastTime = seconds;

          const controls = controlsRef.current;
          let dx = Number(controls.right) - Number(controls.left);
          let dz = Number(controls.down) - Number(controls.up);
          const length = Math.hypot(dx, dz);
          if (length > 0) {
            dx /= length;
            dz /= length;
            playerRef.current[0] = clamp(
              playerRef.current[0] + dx * PLAYER_SPEED * dt,
              -WORLD_LIMIT,
              WORLD_LIMIT,
            );
            playerRef.current[2] = clamp(
              playerRef.current[2] + dz * PLAYER_SPEED * dt,
              -WORLD_LIMIT,
              WORLD_LIMIT,
            );
            playerYaw = Math.atan2(dx, dz);
          }

          const player = playerRef.current;
          const camera = [player[0] + 10.5, 10.5, player[2] + 13.5];
          view = mat4LookAt(camera, [player[0], 1.2, player[2]], [0, 1, 0]);

          gl.viewport(0, 0, gl.drawingBufferWidth, gl.drawingBufferHeight);
          gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
          drawWorld(renderer, {
            player,
            playerYaw,
            missionIndex: missionRef.current,
            time: seconds,
            buildings,
          });
          gl.flush();
          gl.endFrameEXP();
          requestAnimationFrame(render);
        };

        onReady();
        requestAnimationFrame(render);
      } catch (error) {
        onError(error instanceof Error ? error.message : String(error));
      }
    },
    [buildings, controlsRef, missionRef, onError, onReady, playerRef],
  );

  return <GLView style={StyleSheet.absoluteFill} onContextCreate={onContextCreate} />;
}

function HoldButton({ label, direction, controlsRef, style }) {
  const release = useCallback(() => {
    controlsRef.current[direction] = false;
  }, [controlsRef, direction]);

  return (
    <Pressable
      onPressIn={() => {
        controlsRef.current[direction] = true;
      }}
      onPressOut={release}
      onTouchCancel={release}
      style={({ pressed }) => [styles.moveButton, style, pressed && styles.buttonPressed]}
    >
      <Text style={styles.moveText}>{label}</Text>
    </Pressable>
  );
}

function ActionButton({ label, accentStyle, onPress }) {
  return (
    <Pressable onPress={onPress} style={({ pressed }) => [styles.actionButton, accentStyle, pressed && styles.buttonPressed]}>
      <Text style={styles.actionText}>{label}</Text>
    </Pressable>
  );
}

export default function App() {
  const controlsRef = useRef({ up: false, down: false, left: false, right: false });
  const playerRef = useRef([0, 0, 0]);
  const missionRef = useRef(0);
  const [missionIndex, setMissionIndex] = useState(0);
  const [score, setScore] = useState(0);
  const [started, setStarted] = useState(false);
  const [ready, setReady] = useState(false);
  const [renderError, setRenderError] = useState('');
  const [message, setMessage] = useState('Find the first beacon.');
  const [distance, setDistance] = useState(null);

  useEffect(() => {
    const timer = setInterval(() => {
      if (missionRef.current >= MISSIONS.length) {
        setDistance(null);
        return;
      }
      setDistance(distance2D(playerRef.current, MISSIONS[missionRef.current].target));
    }, 200);
    return () => clearInterval(timer);
  }, []);

  const advanceMission = useCallback((action) => {
    const index = missionRef.current;
    if (index >= MISSIONS.length) {
      setMessage('The block is safe. Free-roam unlocked.');
      return;
    }

    const mission = MISSIONS[index];
    const currentDistance = distance2D(playerRef.current, mission.target);
    if (action !== mission.action) {
      setMessage(`Wrong move. Current mission needs ${mission.action.toUpperCase()}.`);
      return;
    }
    if (currentDistance > 4.2) {
      setMessage(`Get closer to the beacon — ${currentDistance.toFixed(1)}m away.`);
      return;
    }

    const nextIndex = index + 1;
    missionRef.current = nextIndex;
    setMissionIndex(nextIndex);
    setScore((current) => current + mission.reward);
    setMessage(
      nextIndex >= MISSIONS.length
        ? 'CEMI RISING! Prototype missions complete.'
        : `Mission clear +${mission.reward}. Next beacon activated.`,
    );
  }, []);

  const mission = missionIndex < MISSIONS.length ? MISSIONS[missionIndex] : null;

  return (
    <View style={styles.root}>
      <StatusBar hidden />
      <GameGL
        controlsRef={controlsRef}
        missionRef={missionRef}
        playerRef={playerRef}
        onReady={() => setReady(true)}
        onError={setRenderError}
      />

      <SafeAreaView pointerEvents="box-none" style={styles.hud}>
        <View style={styles.topRow} pointerEvents="none">
          <View style={styles.brandCard}>
            <Text style={styles.brand}>HERALDS OF THE CEMI</Text>
            <Text style={styles.subtitle}>BRONX FREE-ROAM PROTOTYPE</Text>
          </View>
          <View style={styles.scoreCard}>
            <Text style={styles.scoreLabel}>REP</Text>
            <Text style={styles.scoreValue}>{String(score).padStart(4, '0')}</Text>
          </View>
        </View>

        <View style={styles.missionCard} pointerEvents="none">
          <Text style={styles.missionKicker}>{mission ? `MISSION ${missionIndex + 1}/3` : 'FREE ROAM'}</Text>
          <Text style={styles.missionTitle}>{mission ? mission.title : 'The neighborhood is protected'}</Text>
          <Text style={styles.missionHint}>{mission ? mission.hint : 'Explore the block and test the controls.'}</Text>
          <Text style={styles.distanceText}>
            {mission && distance !== null ? `${distance.toFixed(1)}m to objective` : message}
          </Text>
        </View>

        <View style={styles.bottomRow} pointerEvents="box-none">
          <View style={styles.dpad}>
            <HoldButton label="▲" direction="up" controlsRef={controlsRef} style={styles.upButton} />
            <HoldButton label="◀" direction="left" controlsRef={controlsRef} style={styles.leftButton} />
            <HoldButton label="▶" direction="right" controlsRef={controlsRef} style={styles.rightButton} />
            <HoldButton label="▼" direction="down" controlsRef={controlsRef} style={styles.downButton} />
          </View>

          <View style={styles.messageWrap} pointerEvents="none">
            <Text style={styles.message}>{message}</Text>
          </View>

          <View style={styles.actions}>
            <ActionButton label="TAG" accentStyle={styles.tagButton} onPress={() => advanceMission('tag')} />
            <ActionButton label="BALL" accentStyle={styles.ballButton} onPress={() => advanceMission('ball')} />
            <ActionButton label="POWER" accentStyle={styles.powerButton} onPress={() => advanceMission('power')} />
          </View>
        </View>
      </SafeAreaView>

      {!started && (
        <View style={styles.intro}>
          <View style={styles.introPanel}>
            <Text style={styles.introEyebrow}>PLAYABLE EXPO GO CONCEPT</Text>
            <Text style={styles.introTitle}>Become a Herald.</Text>
            <Text style={styles.introBody}>
              Patrol a stylized Bronx block, paint a CEMI mural, play streetball, and defend the neighborhood.
              This tests the mission loop and touch controls before the full s&amp;box production build.
            </Text>
            <Pressable
              disabled={!ready || Boolean(renderError)}
              onPress={() => {
                setStarted(true);
                setMessage('Reach the violet mural beacon.');
              }}
              style={({ pressed }) => [
                styles.startButton,
                (!ready || renderError) && styles.startDisabled,
                pressed && styles.buttonPressed,
              ]}
            >
              <Text style={styles.startText}>
                {renderError ? 'RENDER ERROR' : ready ? 'START PATROL' : 'LOADING CITY…'}
              </Text>
            </Pressable>
            {renderError ? <Text style={styles.errorText}>{renderError}</Text> : null}
          </View>
        </View>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  root: {
    flex: 1,
    backgroundColor: '#060b1c',
  },
  hud: {
    ...StyleSheet.absoluteFillObject,
    paddingHorizontal: 14,
    paddingVertical: 8,
  },
  topRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
  },
  brandCard: {
    backgroundColor: 'rgba(5, 10, 25, 0.84)',
    borderWidth: 1,
    borderColor: 'rgba(100, 220, 255, 0.52)',
    borderRadius: 12,
    paddingHorizontal: 12,
    paddingVertical: 8,
  },
  brand: {
    color: '#c9f6ff',
    fontSize: 15,
    fontWeight: '900',
    letterSpacing: 1.4,
  },
  subtitle: {
    color: '#65cbea',
    fontSize: 8,
    fontWeight: '700',
    letterSpacing: 1.2,
    marginTop: 2,
  },
  scoreCard: {
    minWidth: 76,
    backgroundColor: 'rgba(5, 10, 25, 0.84)',
    borderWidth: 1,
    borderColor: 'rgba(255, 208, 74, 0.58)',
    borderRadius: 12,
    alignItems: 'center',
    paddingVertical: 6,
  },
  scoreLabel: {
    color: '#ffd04a',
    fontSize: 8,
    fontWeight: '900',
    letterSpacing: 1.6,
  },
  scoreValue: {
    color: '#fff4bd',
    fontSize: 18,
    fontWeight: '900',
  },
  missionCard: {
    position: 'absolute',
    top: 72,
    left: 14,
    width: 285,
    backgroundColor: 'rgba(5, 10, 25, 0.82)',
    borderLeftWidth: 4,
    borderLeftColor: '#a726ff',
    borderRadius: 10,
    padding: 10,
  },
  missionKicker: {
    color: '#b75bff',
    fontSize: 9,
    fontWeight: '900',
    letterSpacing: 1.4,
  },
  missionTitle: {
    color: 'white',
    fontSize: 16,
    fontWeight: '900',
    marginTop: 2,
  },
  missionHint: {
    color: '#c6d0e6',
    fontSize: 10,
    lineHeight: 14,
    marginTop: 3,
  },
  distanceText: {
    color: '#ffd04a',
    fontSize: 10,
    fontWeight: '800',
    marginTop: 5,
  },
  bottomRow: {
    position: 'absolute',
    left: 14,
    right: 14,
    bottom: 10,
    height: 118,
    flexDirection: 'row',
    alignItems: 'flex-end',
    justifyContent: 'space-between',
  },
  dpad: {
    width: 132,
    height: 118,
  },
  moveButton: {
    position: 'absolute',
    width: 48,
    height: 48,
    borderRadius: 15,
    backgroundColor: 'rgba(8, 18, 42, 0.9)',
    borderWidth: 1,
    borderColor: 'rgba(121, 222, 255, 0.7)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  upButton: { top: 0, left: 42 },
  leftButton: { top: 42, left: 0 },
  rightButton: { top: 42, right: 0 },
  downButton: { bottom: 0, left: 42 },
  moveText: {
    color: '#d9f8ff',
    fontSize: 21,
    fontWeight: '900',
  },
  messageWrap: {
    flex: 1,
    alignItems: 'center',
    paddingHorizontal: 12,
    paddingBottom: 8,
  },
  message: {
    maxWidth: 340,
    backgroundColor: 'rgba(5, 10, 25, 0.74)',
    borderRadius: 9,
    paddingHorizontal: 10,
    paddingVertical: 7,
    color: '#eef7ff',
    fontSize: 10,
    fontWeight: '700',
    textAlign: 'center',
  },
  actions: {
    width: 178,
    height: 110,
    justifyContent: 'space-between',
  },
  actionButton: {
    height: 32,
    borderRadius: 11,
    borderWidth: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  tagButton: {
    backgroundColor: 'rgba(136, 25, 220, 0.9)',
    borderColor: '#dc9cff',
  },
  ballButton: {
    backgroundColor: 'rgba(218, 79, 10, 0.92)',
    borderColor: '#ffb368',
  },
  powerButton: {
    backgroundColor: 'rgba(204, 16, 35, 0.92)',
    borderColor: '#ff8190',
  },
  actionText: {
    color: 'white',
    fontSize: 11,
    fontWeight: '900',
    letterSpacing: 1.3,
  },
  buttonPressed: {
    opacity: 0.62,
    transform: [{ scale: 0.96 }],
  },
  intro: {
    ...StyleSheet.absoluteFillObject,
    backgroundColor: 'rgba(2, 5, 15, 0.82)',
    alignItems: 'center',
    justifyContent: 'center',
    padding: 20,
  },
  introPanel: {
    width: '88%',
    maxWidth: 560,
    backgroundColor: 'rgba(8, 16, 38, 0.97)',
    borderWidth: 1,
    borderColor: '#2ac9ef',
    borderRadius: 18,
    padding: 22,
  },
  introEyebrow: {
    color: '#43d9ff',
    fontSize: 10,
    fontWeight: '900',
    letterSpacing: 1.8,
  },
  introTitle: {
    color: 'white',
    fontSize: 30,
    fontWeight: '900',
    marginTop: 5,
  },
  introBody: {
    color: '#c7d1e9',
    fontSize: 13,
    lineHeight: 19,
    marginTop: 9,
  },
  startButton: {
    marginTop: 18,
    height: 45,
    borderRadius: 12,
    backgroundColor: '#17bfe9',
    alignItems: 'center',
    justifyContent: 'center',
  },
  startDisabled: {
    backgroundColor: '#33445b',
  },
  startText: {
    color: '#04101d',
    fontSize: 12,
    fontWeight: '900',
    letterSpacing: 1.4,
  },
  errorText: {
    color: '#ff8b99',
    fontSize: 10,
    marginTop: 9,
  },
});
