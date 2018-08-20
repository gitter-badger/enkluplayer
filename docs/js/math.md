```javascript
// vector constructors
vec2(x, y);
vec3(x, y, z);
vec4(x, y, z, w);

// vector constants
v.one;
v.zero;
v.up;
v.right;
v.forward;

// vector math
v.mult(10, v.one);
v.add(a, b);
v.dot(a, b);
v.cross(a, b);
v.len();
```

```javascript
// quaternion constructors
quat(x, y, z, w);
quat(x, y, z); // infers euler

// quaternion constants
q.identity;

// quaternion math
q.eul(x, y, z);
```