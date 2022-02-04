import {FC, ReactElement, useContext} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {Button, Grid, Theme} from "@material-ui/core";
import {AppContext} from "../App";
import {createStyles, makeStyles} from "@material-ui/core/styles";
import {login} from "../utils/security";

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        content: {
            "& a": {
                color: theme.palette.text.primary
            }
        }
    })
);

const Login: FC<CommonProps> = (props: CommonProps): ReactElement => {

    const classes = useStyles();
    const context = useContext(AppContext);
    context.setAllBookId(null);

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            <Grid container={true} className={classes.content}>
                <Grid item md={4} sm={false} xs={false}/>
                <Grid item md={4} sm={12} xs={12}>
                    <p>
                        You must login to access this feature branch.
                    </p>
                </Grid>
                <Grid item md={4} sm={false} xs={false}/>
                <Grid item md={4} sm={false} xs={false}/>
                <Grid item md={4} sm={12} xs={12}>
                    <Button variant={"outlined"} onClick={_ => login(context.settings.aws.cognitoLogin)}>Login</Button>
                </Grid>
                <Grid item md={4} sm={false} xs={false}/>
            </Grid>

        </>
    );
}

export default Login;