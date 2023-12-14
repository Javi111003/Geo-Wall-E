//Gets a regular hexagon defined by the circle defined by point p and measure m.
//Gets the mediatrix of the line defined by p1 and p2.
mediatrix(p1, p2) = 
    let
        l1 = line(p1, p2);
        m = measure (p1, p2);
        c1 = circle (p1, m);
        c2 = circle (p2, m);
        i1,i2,_ = intersect(c1, c2);
    in line(i1,i2);

m = mediatrix(point(0, 10), point(0, 20));
m;

Fac(n) = if n <= 1 then 1 else n * Fac(n-1);
print Fac(4) "Fac(4)";

Fib(n) = if n <= 1 then 1 else Fib(n-1) + Fib(n-2);
print Fib(6) "Fib(6)"; 


a(n,m) = 
let 
    n1 = n;
    m1 = m;
    x = if n>0 then a(n-1,m+3) else 1;
    print n1;
    print m1;
    print m;
    print n;
in 1;

callA =a(2,3);

circle b;
line c;
point t;
segment e;
point f;
line yj;

draw b;
draw c;
draw t;
draw e;
draw f;
draw yj;


f(u)=
let
    circle b;
    line c;
    point t;
    segment e;
    point f;
    line yg;
    line yj;

    draw b;
    draw c;
    draw t;
    draw e;
    draw f;
    draw yg;
    draw yj;
in if u>0 then f(u-1)else 0;

trtrt= f(3);