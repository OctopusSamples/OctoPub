// icons
import HomeIcon from '@material-ui/icons/Home';

// components
import Home from '../pages/Home';
import Book from '../pages/Book';

// interface
import RouteItem, {CommonProps} from '../model/RouteItem.model';
import Audits from "../pages/Audits";
import AddBook from "../pages/AddBook";
import Settings from "../pages/Settings";
import DeleteBook from "../pages/DeleteBook";

// define app routes
export const routes: Array<RouteItem> = [
    {
        key: "router-home",
        title: "Home",
        tooltip: "Home",
        path: "/",
        enabled: true,
        component: (props: CommonProps) => () => <Home {...props}/>,
        icon: HomeIcon,
        appendDivider: true
    },
    {
        key: "router-home-index",
        title: "Home",
        tooltip: "Home",
        path: "/index.html",
        enabled: true,
        component: (props: CommonProps) => () => <Home {...props}/>,
        icon: HomeIcon,
        appendDivider: true
    },
    {
        key: "router-book",
        title: "Home",
        tooltip: "Home",
        path: "/book/:bookId",
        enabled: true,
        component: (props: CommonProps) => () => <Book {...props}/>,
        icon: HomeIcon,
        appendDivider: true
    },
    {
        key: "router-audits",
        title: "Audits",
        tooltip: "Audits",
        path: "/audits",
        enabled: true,
        component: (props: CommonProps) => () => <Audits {...props}/>,
        icon: HomeIcon,
        appendDivider: true
    },
    {
        key: "router-addbook",
        title: "Add Book",
        tooltip: "Add Book",
        path: "/addBook",
        enabled: true,
        component: (props: CommonProps) => () => <AddBook {...props}/>,
        icon: HomeIcon,
        appendDivider: true
    },
    {
        key: "router-deletebook",
        title: "Delete Book",
        tooltip: "Delete Book",
        path: "/deleteBook/:bookId",
        enabled: true,
        component: (props: CommonProps) => () => <DeleteBook {...props}/>,
        icon: HomeIcon,
        appendDivider: true
    },
    {
        key: "router-settings",
        title: "Settings",
        tooltip: "Settings",
        path: "/settings",
        enabled: true,
        component: (props: CommonProps) => () => <Settings {...props}/>,
        icon: HomeIcon,
        appendDivider: true
    }
]