
//c1 = circle(point(50, 50), 100);
//c2 = circle(point(100, 100), 200);

//point p1;
//point p2;

rev_index(n, k) = if count(n) > -k
       then
           let a, rest = n;
               in rev_index(rest, k)
       else
           let a, rest = n;
               in a;
